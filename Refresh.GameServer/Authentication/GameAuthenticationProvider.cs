using System.Diagnostics;
using System.Net;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;
using Refresh.HttpServer.Authentication;
using Refresh.HttpServer.Database;

namespace Refresh.GameServer.Authentication;

public class GameAuthenticationProvider : IAuthenticationProvider<GameUser>
{
    public GameUser? AuthenticateUser(HttpListenerRequest request, IDatabaseContext db)
    {
        RealmDatabaseContext database = (RealmDatabaseContext)db;
        Debug.Assert(database != null);

        return database.GetUserFromTokenData(request.Headers["Authorization"]);
    }
}