using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Endpoints.ApiV2;

public class LeaderboardApiEndpoints : EndpointGroup
{
    [ApiV2Endpoint("scores/{id}/{type}")]
    [Authentication(false)]
    public List<GameSubmittedScore>? GetTopScoresForLevel(RequestContext context, GameDatabaseContext database, int? id, int? type)
    {
        if (id == null) return null;
        if (type == null) return null;
        
        GameLevel? level = database.GetLevelById(id.Value);
        if (level == null) return null;
        
        (int skip, int count) = context.GetPageData(true);

        bool result = bool.TryParse(context.QueryString.Get("showAll") ?? "false", out bool showAll);
        if (!result) return null; // FIXME: Should return BadRequest.

        return database.GetTopScoresForLevel(level, count, skip, (byte)type, showAll).Items.ToList();
    }

    [ApiV2Endpoint("score/{uuid}")]
    [Authentication(false)]
    public GameSubmittedScore? GetScoreByUuid(RequestContext context, GameDatabaseContext database, string uuid) => database.GetScoreByUuid(uuid);
}