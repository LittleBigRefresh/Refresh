using System.Security.Cryptography;
using JetBrains.Annotations;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class RealmDatabaseContext
{
    private static string GetTokenString()
    {
        byte[] tokenData = new byte[128];
        
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenData);

        return Convert.ToBase64String(tokenData);
    }
    
    public Token GenerateTokenForUser(GameUser user, TokenType type)
    {
        // TODO: JWT (JSON Web Tokens) for TokenType.Api


        Token token = new()
        {
            User = user,
            TokenData = GetTokenString(),
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
            TokenData = GetTokenString(),
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
        string realType = type.ToString();
        return this._realm.All<Token>()
            .FirstOrDefault(t => t.TokenData == tokenData && t._TokenType == realType)?
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