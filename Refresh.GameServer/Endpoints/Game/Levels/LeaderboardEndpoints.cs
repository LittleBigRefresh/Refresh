using System.Net;
using System.Text.Json.Nodes;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Newtonsoft.Json;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Endpoints.Game.Levels;

public class LeaderboardEndpoints : EndpointGroup
{
    [GameEndpoint("play/user/{id}", ContentType.Xml, Method.Post)]
    public Response PlayLevel(RequestContext context, GameUser user, GameDatabaseContext database, int? id)
    {
        if (id == null) return HttpStatusCode.BadRequest;

        GameLevel? level = database.GetLevelById(id.Value);
        if (level == null) return HttpStatusCode.NotFound;

        database.PlayLevel(level, user);
        return HttpStatusCode.OK;
    }

    [GameEndpoint("scoreboard/user/{id}", ContentType.Xml, Method.Post)]
    public Response SubmitScore(RequestContext context, GameUser user, GameDatabaseContext database, int? id, GameScore body)
    {
        if (id == null) return HttpStatusCode.BadRequest;

        GameLevel? level = database.GetLevelById(id.Value);
        if (level == null) return HttpStatusCode.NotFound;
        
        Console.WriteLine(JsonConvert.SerializeObject(body));
        
        bool result = database.SubmitScore(body, user, level);
        
        if (result) return HttpStatusCode.OK;
        return HttpStatusCode.Unauthorized;
    }
}