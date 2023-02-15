using System.Diagnostics;
using System.Net;
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

        return database.GetUserFromTokenData(request.Cookies["MM_AUTH"]);
    }
}