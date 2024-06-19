using System.Security.Cryptography;
using JetBrains.Annotations;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

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
            this.CreateUserFirstLoginEvent(user, user);
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
            .FirstOrDefault(t => t.TokenData == tokenData && t._TokenType == (int)type);

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

        Token? token = this.Tokens.FirstOrDefault(t => t.TokenData == tokenData && t._TokenType == (int)type);
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
            this.Tokens.RemoveRange(t => t.User == user && t._TokenType == (int)type);
        });
    }
    
    public bool IsTokenExpired(Token token) => token.ExpiresAt < this._time.Now;
    
    public DatabaseList<Token> GetAllTokens()
        => new(this.Tokens);

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

    public void SetApprovedIp(GameUser user, string ipAddress)
    {
        this.Write(() =>
        {
            user.CurrentVerifiedIp = ipAddress;
            this.GameIpVerificationRequests.RemoveRange(this.GameIpVerificationRequests.Where(r => r.User == user));
        });
    }

    public void DenyIpVerificationRequest(GameUser user, string ipAddress)
    {
        this.Write(() =>
        {
            this.GameIpVerificationRequests.RemoveRange(this.GameIpVerificationRequests.Where(r => r.IpAddress == ipAddress && r.User == user));
        });
    }

    public DatabaseList<GameIpVerificationRequest> GetIpVerificationRequestsForUser(GameUser user, int count, int skip) 
        => new(this.GameIpVerificationRequests.Where(r => r.User == user), skip, count);
}