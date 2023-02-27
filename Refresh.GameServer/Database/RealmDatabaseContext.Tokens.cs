using JetBrains.Annotations;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Database;

public partial class RealmDatabaseContext
{
    public Token GenerateTokenForUser(GameUser user)
    {
        Token token = new()
        {
            User = user,
            TokenData = Guid.NewGuid().ToString(),
        };

        this._realm.Write(() =>
        {
            this._realm.Add(token);
        });
        
        return token;
    }

    [Pure]
    [ContractAnnotation("null => null; notnull => canbenull")]
    public GameUser? GetUserFromTokenData(string? tokenData)
    {
        if (tokenData == null) return null;
        return this._realm.All<Token>().FirstOrDefault(t => t.TokenData == tokenData)?.User;
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