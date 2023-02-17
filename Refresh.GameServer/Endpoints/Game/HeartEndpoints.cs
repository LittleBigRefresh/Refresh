using System.Net;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game;

public class HeartEndpoints : EndpointGroup
{
    [GameEndpoint("favourite/slot/user/{idStr}", Method.Post)]
    public Response HeartLevel(RequestContext context, RealmDatabaseContext database, GameUser user, string idStr)
    {
        int.TryParse(idStr, out int id);
        if (id == default) return new Response(HttpStatusCode.BadRequest);
        
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return new Response(HttpStatusCode.NotFound);

        if (database.HeartLevel(level, user))
            return new Response(HttpStatusCode.OK);
        
        return new Response(HttpStatusCode.Unauthorized);
    }
}