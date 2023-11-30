using System.Security.Cryptography;
using JetBrains.Annotations;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial interface IGameDatabaseContext // Tokens
{
    private const int DefaultCookieLength = 128;
    private const int MaxBase64Padding = 4;
    private const int MaxGameCookieLength = 127;
    private const string GameCookieHeader = "MM_AUTH=";
    private static readonly int GameCookieLength = (int)Math.Floor((MaxGameCookieLength - GameCookieHeader.Length - MaxBase64Padding) * 3 / 4.0);

    public const int DefaultTokenExpirySeconds = 86400; // 1 day
    public const int RefreshTokenExpirySeconds = 2678400; // 1 month
    public const int GameTokenExpirySeconds = 14400; // 4 hours
    
    private static string GetTokenString(int length)
    {
        byte[] tokenData = new byte[length];
        
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenData);

        return Convert.ToBase64String(tokenData);
    }
    
    public Token GenerateTokenForUser(GameUser user, TokenType type, TokenGame game, TokenPlatform platform, int tokenExpirySeconds = DefaultTokenExpirySeconds)
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
            ExpiresAt = this.Time.Now.AddSeconds(tokenExpirySeconds),
            LoginDate = this.Time.Now,
        };
        
        if (user.LastLoginDate == DateTimeOffset.MinValue)
        {
            this.CreateUserFirstLoginEvent(user, user);
        }

        this.Write(() =>
        {
            user.LastLoginDate = this.Time.Now;
            this.Add(token);
        });
        
        return token;
    }
    
    [Pure]
    [ContractAnnotation("=> canbenull")]
    public Token? GetTokenFromTokenData(string tokenData, TokenType type)
    {
        Token? token = this.All<Token>()
            .FirstOrDefault(t => t.TokenData == tokenData && t._TokenType == (int)type);

        if (token == null) return null;

        // ReSharper disable once InvertIf
        if (token.ExpiresAt < this.Time.Now)
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

        Token? token = this.All<Token>().FirstOrDefault(t => t.TokenData == tokenData && t._TokenType == (int)type);
        if (token == null) return false;

        this.RevokeToken(token);

        return true;
    }

    public void RevokeToken(Token token)
    {
        this.Write(() =>
        {
            this.Remove(token);
        });
    }

    public void RevokeAllTokensForUser(GameUser user)
    {
        this.Write(() =>
        {
            this.RemoveRange(this.All<Token>().Where(t => t.User == user));
        });
    }

    public void AddIpVerificationRequest(GameUser user, string ipAddress)
    {
        GameIpVerificationRequest request = new()
        {
            IpAddress = ipAddress,
            CreatedAt = this.Time.Now,
        };

        this.Write(() =>
        {
            user.IpVerificationRequests.Add(request);
        });
    }

    public void SetApprovedIp(GameUser user, string ipAddress)
    {
        this.Write(() =>
        {
            user.CurrentVerifiedIp = ipAddress;
            user.IpVerificationRequests.Clear();
        });
    }

    public void DenyIpVerificationRequest(GameUser user, string ipAddress)
    {
        IEnumerable<GameIpVerificationRequest> requests = user.IpVerificationRequests.Where(r => r.IpAddress == ipAddress);
        this.Write(() =>
        {
            foreach (GameIpVerificationRequest request in requests)
            {
                user.IpVerificationRequests.Remove(request);
            }
        });
    }

    public DatabaseList<GameIpVerificationRequest> GetIpVerificationRequestsForUser(GameUser user, int count, int skip) 
        => new(user.IpVerificationRequests, skip, count);
}