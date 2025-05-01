using System.Diagnostics;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Endpoints.Debugging;
using Bunkum.Core.RateLimit;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.Scores;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Endpoints.Game.Levels;

public class LeaderboardEndpoints : EndpointGroup
{
    private const int RequestTimeoutDuration = 300;
    private const int MaxRequestAmount = 250;
    private const int RequestBlockDuration = 300;
    private const string BucketName = "score";

    [GameEndpoint("play/{slotType}/{id}", ContentType.Xml, HttpMethods.Post)]
    public Response PlayLevel(RequestContext context, GameUser user, GameDatabaseContext database, string slotType, int id)
    {
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        if (level == null) return NotFound;

        int count = 1;
        //If we are on PSP, and it has sent a `count` parameter...
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
    
    [GameEndpoint("scoreboard/{slotType}/{id}", HttpMethods.Get, ContentType.Xml)]
    [RateLimitSettings(RequestTimeoutDuration, MaxRequestAmount, RequestBlockDuration, BucketName)]
    public Response GetUserScores(RequestContext context, GameUser user, GameDatabaseContext database, string slotType, int id, Token token)
    {
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        if (level == null) return NotFound;
        
        //Get the scores from the database
        MultiLeaderboard multiLeaderboard = new(database, level, token.TokenGame);
        
        return new Response(SerializedMultiLeaderboardResponse.FromOld(multiLeaderboard), ContentType.Xml);
    }

    [GameEndpoint("scoreboard/friends/{slotType}/{id}", HttpMethods.Post, ContentType.Xml)]
    [RateLimitSettings(RequestTimeoutDuration, MaxRequestAmount, RequestBlockDuration, BucketName)]
    [NullStatusCode(NotFound)]
    public SerializedScoreLeaderboardList? GetLevelFriendLeaderboard(
        RequestContext context, 
        GameUser user, 
        GameDatabaseContext database, 
        string slotType, 
        int id, 
        FriendScoresRequest body)
    {
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        if (level == null) return null;
        
        return SerializedScoreLeaderboardList.FromSubmittedEnumerable(database.GetLevelTopScoresByFriends(user, level, 10, body.Type));
    }
    
    [GameEndpoint("scoreboard/{slotType}/{id}", ContentType.Xml, HttpMethods.Post)]
    [RateLimitSettings(RequestTimeoutDuration, MaxRequestAmount, RequestBlockDuration, BucketName)]
    [RequireEmailVerified]
    public Response SubmitScore(RequestContext context, GameUser user, GameServerConfig config, GameDatabaseContext database, string slotType, int id, SerializedScore body, Token token)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;
        
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        if (level == null) return NotFound;

        // Validate the score is a non-negative amount
        if (body.Score < 0)
        {
            return BadRequest;
        }
        
        // Ensure score type is valid
        // Only valid values are 1-4 players and 7 for versus
        if (body.ScoreType is (> 4 or < 1) and not 7)
        {
            return BadRequest;
        }

        GameSubmittedScore score = database.SubmitScore(body, token, level);

        IEnumerable<ScoreWithRank>? scores = database.GetRankedScoresAroundScore(score, 5);
        Debug.Assert(scores != null);
        
        return new Response(SerializedScoreLeaderboardList.FromSubmittedEnumerable(scores), ContentType.Xml);
    }

    [GameEndpoint("topscores/{slotType}/{id}/{type}", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [RateLimitSettings(RequestTimeoutDuration, MaxRequestAmount, RequestBlockDuration, BucketName)]
    public Response GetTopScoresForLevel(RequestContext context, GameDatabaseContext database, string slotType, int id, int type)
    {
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        if (level == null) return NotFound;
        
        (int skip, int count) = context.GetPageData();
        return new Response(SerializedScoreList.FromSubmittedEnumerable(database.GetTopScoresForLevel(level, count, skip, (byte)type).Items, skip), ContentType.Xml);
    }
}