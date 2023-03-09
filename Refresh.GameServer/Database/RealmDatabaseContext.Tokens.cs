using JetBrains.Annotations;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class RealmDatabaseContext
{
    public Token GenerateTokenForUser(GameUser user, TokenType type)
    {
        Token token = new()
        {
            User = user,
            TokenData = Guid.NewGuid().ToString(),
            TokenType = type,
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
            .FirstOrDefault(t => t.TokenData == tokenData && t.TokenType == type)?
            .User;
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