using System.Security.Cryptography;
using JetBrains.Annotations;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext
{
    private const int DefaultCookieLength = 128;
    private const int MaxBase64Padding = 4;
    private const int MaxGameCookieLength = 127;
    private const string GameCookieHeader = "MM_AUTH=";
    private static readonly int GameCookieLength;

    private const int DefaultTokenExpirySeconds = 86400; // 1 day
    
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
    
    public Token GenerateTokenForUser(GameUser user, TokenType type, TokenGame game, TokenPlatform platform, int? tokenExpirySeconds = null)
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
            ExpiresAt = DateTimeOffset.Now.AddSeconds(tokenExpirySeconds ?? DefaultTokenExpirySeconds),
        };

        this._realm.Write(() =>
        {
            this._realm.Add(token);
        });
        
        return token;
    }

    [Pure]
    [ContractAnnotation("=> canbenull")]
    public GameUser? GetUserFromTokenData(string tokenData, TokenType type)
    {
        Token? token = this._realm.All<Token>()
            .FirstOrDefault(t => t.TokenData == tokenData && t._TokenType == (int)type);

        if (token == null) return null;

        // ReSharper disable once InvertIf
        if (token.ExpiresAt < DateTimeOffset.Now)
        {
            this._realm.Write(() => this._realm.Remove(token));
            return null;
        }

        return token.User;
    }

    public void SetUserPassword(GameUser user, string passwordBcrypt)
    {
        this._realm.Write(() =>
        {
            user.PasswordBcrypt = passwordBcrypt;
        });
    }

    public bool RevokeTokenByTokenData(string? tokenData, TokenType type)
    {
        if (tokenData == null) return false;

        Token? token = this._realm.All<Token>().FirstOrDefault(t => t.TokenData == tokenData && t._TokenType == (int)type);
        if (token == null) return false;

        this._realm.Write(() =>
        {
            this._realm.Remove(token);
        });

        return true;
    }
}