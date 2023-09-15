using System.Diagnostics;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Endpoints.Game.Levels;

public class LeaderboardEndpoints : EndpointGroup
{
    [GameEndpoint("play/user/{id}", ContentType.Xml, Method.Post)]
    public Response PlayLevel(RequestContext context, GameUser user, GameDatabaseContext database, int id)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return NotFound;

        database.PlayLevel(level, user);
        return OK;
    }

    [GameEndpoint("scoreboard/developer/{id}", Method.Get, ContentType.Xml)]
    public SerializedLeaderboardResponse GetDeveloperScores(RequestContext context, GameUser user, GameDatabaseContext database, int id)
    {
        //TODO
        return new SerializedLeaderboardResponse(new List<SerializedPlayerLeaderboardResponse>());
    }

    [GameEndpoint("scoreboard/user/{id}", Method.Get, ContentType.Xml)]
    public SerializedLeaderboardResponse GetUserScores(RequestContext context, GameUser user, GameDatabaseContext database, int id)
    {
        //TODO
        return new SerializedLeaderboardResponse(new List<SerializedPlayerLeaderboardResponse>());
    }

    [GameEndpoint("scoreboard/developer/{id}", ContentType.Xml, Method.Post)]
    public Response SubmitDeveloperScore(RequestContext context, GameUser user, GameDatabaseContext database, int id, SerializedScore body)
    {
        //TODO
        return new Response(SerializedScoreLeaderboardList.FromSubmittedEnumerable(new List<ScoreWithRank>()), ContentType.Xml);
    }

    [GameEndpoint("scoreboard/user/{id}", ContentType.Xml, Method.Post)]
    public Response SubmitScore(RequestContext context, GameUser user, GameDatabaseContext database, int id, SerializedScore body)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return NotFound;

        //Validate the score is a non-negative amount
        if (body.Score < 0)
        {
            return BadRequest;
        }

        GameSubmittedScore score = database.SubmitScore(body, user, level);

        IEnumerable<ScoreWithRank>? scores = database.GetRankedScoresAroundScore(score, 5);
        Debug.Assert(scores != null);
        
        return new Response(SerializedScoreLeaderboardList.FromSubmittedEnumerable(scores), ContentType.Xml);
    }

    [GameEndpoint("topscores/user/{id}/{type}", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public SerializedScoreList? GetTopScoresForLevel(RequestContext context, GameDatabaseContext database, int id, int type)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return null;
        
        (int skip, int count) = context.GetPageData();
        return SerializedScoreList.FromSubmittedEnumerable(database.GetTopScoresForLevel(level, count, skip, (byte)type).Items);
    }
}