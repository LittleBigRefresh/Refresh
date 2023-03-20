using System.Security.Cryptography;
using JetBrains.Annotations;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class RealmDatabaseContext
{
    private const int DefaultCookieLength = 128;
    private const int MaxBase64Padding = 4;
    private const int MaxGameCookieLength = 127;
    private const string GameCookieHeader = "MM_AUTH=";
    private static readonly int GameCookieLength;

    static RealmDatabaseContext()
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
    
    public Token GenerateTokenForUser(GameUser user, TokenType type)
    {
        // TODO: JWT (JSON Web Tokens) for TokenType.Api
        
        int cookieLength = type == TokenType.Game ? GameCookieLength : DefaultCookieLength;

        Token token = new()
        {
            User = user,
            TokenData = GetTokenString(cookieLength),
            TokenType = type,
        };

        this._realm.Write(() =>
        {
            this._realm.Add(token);
        });
        
        return token;
    }
    
    public ResetToken GenerateResetTokenForUser(GameUser user)
    {
        ResetToken token = new()
        {
            User = user,
            TokenData = GetTokenString(DefaultCookieLength),
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
        return this._realm.All<Token>()
            .FirstOrDefault(t => t.TokenData == tokenData && t._TokenType == (int)type)?
            .User;
    }
    
    [Pure]
    [ContractAnnotation("=> canbenull")]
    public GameUser? GetUserFromResetTokenData(string tokenData)
    {
        return this._realm.All<ResetToken>()
            .FirstOrDefault(t => t.TokenData == tokenData)?
            .User;
    }

    public void SetUserPassword(GameUser user, string passwordBcrypt)
    {
        this._realm.Write(() =>
        {
            user.PasswordBcrypt = passwordBcrypt;
        });
    }

    public bool RevokeTokenByTokenData(string? tokenData)
    {
        if (tokenData == null) return false;

        Token? token = this._realm.All<Token>().FirstOrDefault(t => t.TokenData == tokenData);
        if (token == null) return false;

        this._realm.Write(() =>
        {
            this._realm.Remove(token);
        });

        return true;
    }
}