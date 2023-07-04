using System.Diagnostics;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Endpoints.Game.Levels;

public class LeaderboardEndpoints : EndpointGroup
{
    [GameEndpoint("play/user/{id}", ContentType.Xml, Method.Post)]
    public Response PlayLevel(RequestContext context, GameUser user, GameDatabaseContext database, int? id)
    {
        if (id == null) return BadRequest;

        GameLevel? level = database.GetLevelById(id.Value);
        if (level == null) return NotFound;

        database.PlayLevel(level, user);
        return OK;
    }

    [GameEndpoint("scoreboard/user/{id}", ContentType.Xml, Method.Post)]
    public Response SubmitScore(RequestContext context, GameUser user, GameDatabaseContext database, int? id, SerializedScore body)
    {
        if (id == null) return BadRequest;

        GameLevel? level = database.GetLevelById(id.Value);
        if (level == null) return NotFound;

        GameSubmittedScore? score = database.SubmitScore(body, user, level);
        if (score == null) return Unauthorized;

        IEnumerable<ScoreWithRank>? scores = database.GetRankedScoresAroundScore(score, 5);
        Debug.Assert(scores != null);
        
        return new Response(SerializedScoreLeaderboardList.FromSubmittedEnumerable(scores), ContentType.Xml);
    }

    [GameEndpoint("topscores/user/{id}/{type}", ContentType.Xml)]
    public SerializedScoreList? GetTopScoresForLevel(RequestContext context, GameDatabaseContext database, int? id, int? type)
    {
        if (id == null) return null;
        if (type == null) return null;
        
        GameLevel? level = database.GetLevelById(id.Value);
        if (level == null) return null;
        
        (int skip, int count) = context.GetPageData();
        return SerializedScoreList.FromSubmittedEnumerable(database.GetTopScoresForLevel(level, count, skip, (byte)type).Items);
    }
}