using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Reviews;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game;

public class ReviewEndpoints : EndpointGroup
{
    [GameEndpoint("dpadrate/{type}/{levelId}", Method.Post)]
    public Response SubmitRating(RequestContext context, GameDatabaseContext database, GameUser user, int levelId)
    {
        GameLevel? level = database.GetLevelById(levelId);
        if (level == null) return NotFound;

        bool parsed = sbyte.TryParse(context.QueryString.Get("rating"), out sbyte rating);
        if (!parsed) return BadRequest;

        bool rated = database.RateLevel(level, user, (RatingType)rating);
        return rated ? OK : Unauthorized;
    }
}