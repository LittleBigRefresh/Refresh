using System.Security.Cryptography;
using JetBrains.Annotations;
using Refresh.Common.Time;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Relations;

namespace Refresh.Database;

public partial class GameDatabaseContext // Tokens
{
    private const int DefaultCookieLength = 128;
    private const int MaxBase64Padding = 4;
    private const int MaxGameCookieLength = 127;
    private const string GameCookieHeader = "MM_AUTH=";
    private static readonly int GameCookieLength;

    public const int DefaultTokenExpirySeconds = 86400; // 1 day
    public const int RefreshTokenExpirySeconds = 2678400; // 1 month
    public const int GameTokenExpirySeconds = 14400; // 4 hours
    
    static GameDatabaseContext()
    {
        // LBP cannot store tokens if >127 chars, calculate max possible length here
        GameCookieLength = (int)Math.Floor((MaxGameCookieLength - GameCookieHeader.Length - MaxBase64Padding) * 3 / 4.0);
    }
    
    private static string GetTokenString(int length)
    {
        byte[] tokenData = new byte[length];
        
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenData);

        return Convert.ToBase64String(tokenData);
    }
    
    public Token GenerateTokenForUser(GameUser user, TokenType type, TokenGame game, TokenPlatform platform, string ipAddress, int tokenExpirySeconds = DefaultTokenExpirySeconds)
    {
        // TODO: JWT (JSON Web Tokens) for TokenType.Api
        
        int cookieLength = type == TokenType.Game ? GameCookieLength : DefaultCookieLength;

        Token token = new()
        {
            User = user,
            TokenData = GetTokenString(cookieLength),
            TokenType = type,
            TokenGame = game,
            TokenPlatform = platform,
            ExpiresAt = this._time.Now.AddSeconds(tokenExpirySeconds),
            LoginDate = this._time.Now,
            IpAddress = ipAddress,
        };
        
        if (user.LastLoginDate == DateTimeOffset.MinValue)
        {
            this.CreateUserFirstLoginEvent(user);
        }

        this.Write(() =>
        {
            user.LastLoginDate = this._time.Now;
            this.Tokens.Add(token);
        });
        
        return token;
    }
    
    [Pure]
    [ContractAnnotation("=> canbenull")]
    public Token? GetTokenFromTokenData(string tokenData, TokenType type)
    {
        Token? token = this.Tokens
            .Include(t => t.User)
            .FirstOrDefault(t => t.TokenData == tokenData && t.TokenType == type);

        if (token == null) return null;

        // ReSharper disable once InvertIf
        if (token.ExpiresAt < this._time.Now)
        {
            this.RevokeToken(token);
            return null;
        }

        return token;
    }

    [Pure]
    [ContractAnnotation("=> canbenull")]
    public GameUser? GetUserFromTokenData(string tokenData, TokenType type) => 
        this.GetTokenFromTokenData(tokenData, type)?.User;

    public void SetUserPassword(GameUser user, string? passwordBcrypt, bool shouldReset = false)
    {
        this.Write(() =>
        {
            user.PasswordBcrypt = passwordBcrypt;
            user.ShouldResetPassword = shouldReset;
        });
    }

    public bool RevokeTokenByTokenData(string? tokenData, TokenType type)
    {
        if (tokenData == null) return false;

        Token? token = this.Tokens.FirstOrDefault(t => t.TokenData == tokenData && t.TokenType == type);
        if (token == null) return false;

        this.RevokeToken(token);

        return true;
    }

    public void RevokeToken(Token token)
    {
        this.Write(() =>
        {
            this.Tokens.Remove(token);
        });
    }

    public void RevokeAllTokensForUser(GameUser user)
    {
        this.Write(() =>
        {
            this.Tokens.RemoveRange(t => t.User == user);
        });
    }
    
    public void RevokeAllTokensForUser(GameUser user, TokenType type)
    {
        this.Write(() =>
        {
            this.Tokens.RemoveRange(t => t.User == user && t.TokenType == type);
        });
    }
    
    public bool IsTokenExpired(Token token) => token.ExpiresAt < this._time.Now;
    
    public DatabaseList<Token> GetAllTokens()
        => new(this.Tokens.Include(t => t.User));
    
    public void ResetApiRefreshTokenExpiry(Token token)
    {
        if (token.TokenType != TokenType.ApiRefresh)
            throw new InvalidOperationException("Cannot update a non-refresh token's expiry date");
        
        this.Write(() =>
        {
            token.ExpiresAt = this._time.Now.AddSeconds(RefreshTokenExpirySeconds);
        });
    }

    public void AddIpVerificationRequest(GameUser user, string ipAddress)
    {
        GameIpVerificationRequest request = new()
        {
            User = user,
            IpAddress = ipAddress,
            CreatedAt = this._time.Now,
        };

        this.Write(() =>
        {
            this.GameIpVerificationRequests.Add(request);
        });
    }

    public void AddVerifiedIp(GameUser user, string ipAddress, IDateTimeProvider timeProvider)
    {
        const int maxVerifiedIps = 3;
        
        int count = this.GameUserVerifiedIpRelations.Count(r => r.User == user);
        int toRemove = count >= maxVerifiedIps ? count - maxVerifiedIps + 1 : 0;
        
        this.Write(() =>
        {
            // Remove the oldest verified IPs if the user has too many (or will have too many after this one)
            if (toRemove > 0)
                this.GameUserVerifiedIpRelations.RemoveRange(
                    this.GameUserVerifiedIpRelations
                        .Where(r => r.User == user)
                        .OrderBy(r => r.VerifiedAt)
                        .AsEnumerable()
                        .Take(toRemove));
            
            this.GameUserVerifiedIpRelations.Add(new GameUserVerifiedIpRelation
            {
                User = user,
                IpAddress = ipAddress,
                VerifiedAt = timeProvider.Now,
            });
            
            this.GameIpVerificationRequests.RemoveRange(r => r.User == user && r.IpAddress == ipAddress);
        });
    }

    public bool RemoveVerifiedIp(GameUser user, string ipAddress)
    {
        GameUserVerifiedIpRelation? verifiedIp =
            this.GameUserVerifiedIpRelations.FirstOrDefault(r => r.User == user && r.IpAddress == ipAddress);

        if (verifiedIp == null)
            return false;
        
        this.Write(() =>
        {
            this.GameUserVerifiedIpRelations.Remove(verifiedIp);
        });

        return true;
    }

    public DatabaseList<GameUserVerifiedIpRelation> GetVerifiedIps(GameUser user, int skip, int count) 
        => new(this.GameUserVerifiedIpRelations.Where(r => r.User == user), skip, count);

    public bool IsIpVerified(GameUser user, string ipAddress) 
        => this.GameUserVerifiedIpRelations.Any(r => r.User == user && r.IpAddress == ipAddress);

    public void DenyIpVerificationRequest(GameUser user, string ipAddress)
    {
        this.Write(() =>
        {
            this.GameIpVerificationRequests.RemoveRange(r => r.IpAddress == ipAddress && r.User == user);
        });
    }

    public void SetTokenDigestInfo(Token token, string digest, bool isHmacDigest)
    {
        this.Write(() =>
        {
            token.Digest = digest;
            token.IsHmacDigest = isHmacDigest;
        });
    }

    public DatabaseList<GameIpVerificationRequest> GetIpVerificationRequestsForUser(GameUser user, int count, int skip) 
        => new(this.GameIpVerificationRequests.Where(r => r.User == user), skip, count);
}