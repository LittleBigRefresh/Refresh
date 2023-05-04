using System.Net;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game.Levels;

public class LeaderboardEndpoints : EndpointGroup
{
    [GameEndpoint("play/user/{id}", ContentType.Xml, Method.Post)]
    public Response PlayLevel(RequestContext context, GameUser user, GameDatabaseContext database, int? id)
    {
        if (id == null) return HttpStatusCode.BadRequest;

        GameLevel? level = database.GetLevelById(id.Value);
        if (level == null) return HttpStatusCode.NotFound;

        bool result = database.PlayLevel(level, user);
        if(result) return HttpStatusCode.OK;

        return HttpStatusCode.Unauthorized;
    }
}