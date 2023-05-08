using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Endpoints.Api;

public class LeaderboardApiEndpoints : EndpointGroup
{
    [ApiEndpoint("scores/{id}")]
    [Authentication(false)]
    public List<GameSubmittedScore>? GetTopScoresForLevel(RequestContext context, GameDatabaseContext database, int? id)
    {
        if (id == null) return null;
        
        GameLevel? level = database.GetLevelById(id.Value);
        if (level == null) return null;
        
        (int skip, int count) = context.GetPageData(true);

        bool result = bool.TryParse(context.QueryString.Get("showAll") ?? "false", out bool showAll);
        if (!result) return null; // FIXME: Should return BadRequest.

        return database.GetTopScoresForLevel(level, count, skip, !showAll).ToList();
    }

    [ApiEndpoint("score/{uuid}")]
    [Authentication(false)]
    public GameSubmittedScore? GetScoreByUuid(RequestContext context, GameDatabaseContext database, string uuid) => database.GetScoreByUuid(uuid);
}