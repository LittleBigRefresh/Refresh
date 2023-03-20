using System.Diagnostics;
using Bunkum.CustomHttpListener.Request;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;
using Bunkum.HttpServer.Authentication;
using Bunkum.HttpServer.Database;

namespace Refresh.GameServer.Authentication;

public class GameAuthenticationProvider : IAuthenticationProvider<GameUser>
{
    public GameUser? AuthenticateUser(ListenerContext request, IDatabaseContext db)
    {
        RealmDatabaseContext database = (RealmDatabaseContext)db;
        Debug.Assert(database != null);

        // first try to grab token data from MM_AUTH
        string? tokenData = request.Cookies["MM_AUTH"];
        TokenType type = TokenType.Game;

        // if this is null, this must be an API request so grab from authorization
        if (tokenData == null)
        {
            type = TokenType.Api;
            tokenData = request.RequestHeaders["Authorization"];
        }

        // if still null, then we dont have a token so bail 
        if (tokenData == null) return null;

        return database.GetUserFromTokenData(tokenData, type);
    }
}