using System.Net;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Matching;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Api;

public class MatchingApiEndpoints : EndpointGroup
{
    [ApiEndpoint("room/username/{username}")]
    [Authentication(false)]
    [NullStatusCode(HttpStatusCode.NotFound)]
    public GameRoom? GetRoomByUsername(RequestContext context, MatchService service, GameDatabaseContext database,
        string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return null;

        return service.GetRoomByPlayer(database, user);
    }
}