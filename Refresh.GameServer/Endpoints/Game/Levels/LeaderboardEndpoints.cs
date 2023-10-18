using System.Diagnostics;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.RateLimit;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Authentication;
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
    private const int RequestTimeoutDuration = 300;
    private const int MaxRequestAmount = 250;
    private const int RequestBlockDuration = 300;
    private const string BucketName = "score";
    
    [GameEndpoint("play/user/{id}", ContentType.Xml, HttpMethods.Post)]
    public Response PlayLevel(RequestContext context, GameUser user, GameDatabaseContext database, int id)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return NotFound;

        int count = 1;
        //If we are on PSP and it has sent a `count` parameter...
        if (context.QueryString.Get("count") != null)
        {
            //Count parameters are invalid on non-PSP clients
            if (!context.IsPSP()) return BadRequest;
            
            //Parse the count
            if (!int.TryParse(context.QueryString["count"], out count))
            {
                //If it fails, send a bad request back to the client
                return BadRequest;
            }

            //Sanitize against invalid values
            if (count < 1)
            {
                return BadRequest;
            }
        }
        
        database.PlayLevel(level, user, count);
        return OK;
    }

    [GameEndpoint("scoreboard/developer/{id}", HttpMethods.Get, ContentType.Xml)]
    [RateLimitSettings(RequestTimeoutDuration, MaxRequestAmount, RequestBlockDuration, BucketName)]
    public Response GetDeveloperScores(RequestContext context, GameUser user, GameDatabaseContext database, int id, Token token)
    {
        //No story levels have an ID < 0
        if (id < 0)
        {
            return BadRequest;
        }
        
        GameLevel level = database.GetStoryLevelById(id);

        MultiLeaderboard multiLeaderboard = new(database, level, token.TokenGame);
        
        return new Response(SerializedMultiLeaderboardResponse.FromOld(multiLeaderboard), ContentType.Xml);
    }

    [GameEndpoint("scoreboard/developer/{id}", ContentType.Xml, HttpMethods.Post)]
    [RateLimitSettings(RequestTimeoutDuration, MaxRequestAmount, RequestBlockDuration, BucketName)]
    public Response SubmitDeveloperScore(RequestContext context, GameUser user, GameDatabaseContext database, int id, SerializedScore body, Token token)
    {
        //No story levels have an ID < 0
        if (id < 0)
        {
            return BadRequest;
        }
        
        GameLevel level = database.GetStoryLevelById(id);

        //Validate the score is a non-negative amount
        if (body.Score < 0)
        {
            return BadRequest;
        }

        GameSubmittedScore score = database.SubmitScore(body, user, level, token.TokenGame);

        IEnumerable<ScoreWithRank>? scores = database.GetRankedScoresAroundScore(score, 5);
        Debug.Assert(scores != null);
        
        return new Response(SerializedScoreLeaderboardList.FromSubmittedEnumerable(scores), ContentType.Xml);
    }
    
    [GameEndpoint("scoreboard/user/{id}", HttpMethods.Get, ContentType.Xml)]
    [RateLimitSettings(RequestTimeoutDuration, MaxRequestAmount, RequestBlockDuration, BucketName)]
    public Response GetUserScores(RequestContext context, GameUser user, GameDatabaseContext database, int id, Token token)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return NotFound;
        
        //Get the scores from the database
        MultiLeaderboard multiLeaderboard = new(database, level, token.TokenGame);
        
        return new Response(SerializedMultiLeaderboardResponse.FromOld(multiLeaderboard), ContentType.Xml);
    }
    
    [GameEndpoint("scoreboard/user/{id}", ContentType.Xml, HttpMethods.Post)]
    [RateLimitSettings(RequestTimeoutDuration, MaxRequestAmount, RequestBlockDuration, BucketName)]
    public Response SubmitScore(RequestContext context, GameUser user, GameDatabaseContext database, int id, SerializedScore body, Token token)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return NotFound;

        //Validate the score is a non-negative amount
        if (body.Score < 0)
        {
            return BadRequest;
        }

        GameSubmittedScore score = database.SubmitScore(body, user, level, token.TokenGame);

        IEnumerable<ScoreWithRank>? scores = database.GetRankedScoresAroundScore(score, 5);
        Debug.Assert(scores != null);
        
        return new Response(SerializedScoreLeaderboardList.FromSubmittedEnumerable(scores), ContentType.Xml);
    }

    [GameEndpoint("topscores/user/{id}/{type}", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [RateLimitSettings(RequestTimeoutDuration, MaxRequestAmount, RequestBlockDuration, BucketName)]
    public Response GetTopScoresForLevel(RequestContext context, GameDatabaseContext database, int id, int type)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return NotFound;
        
        (int skip, int count) = context.GetPageData();
        return new Response(SerializedScoreList.FromSubmittedEnumerable(database.GetTopScoresForLevel(level, count, skip, (byte)type).Items), ContentType.Xml);
    }
    
    [GameEndpoint("topscores/developer/{id}/{type}", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [RateLimitSettings(RequestTimeoutDuration, MaxRequestAmount, RequestBlockDuration, BucketName)]
    public Response GetTopScoresForDeveloperLevel(RequestContext context, GameDatabaseContext database, int id, int type)
    {
        //No story levels have an ID < 0
        if (id < 0)
        {
            return BadRequest;
        }
        
        GameLevel level = database.GetStoryLevelById(id);
        
        (int skip, int count) = context.GetPageData();
        return new Response(SerializedScoreList.FromSubmittedEnumerable(database.GetTopScoresForLevel(level, count, skip, (byte)type).Items), ContentType.Xml);
    }
}