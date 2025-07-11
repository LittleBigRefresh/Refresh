using System.Diagnostics;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.RateLimit;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Configuration;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Types.Lists;
using Refresh.Interfaces.Game.Types.Scores;
using Refresh.Interfaces.Game.Types.UserData.Leaderboard;

namespace Refresh.Interfaces.Game.Endpoints.Levels;

public class LeaderboardEndpoints : EndpointGroup
{
    private const int RequestTimeoutDuration = 300;
    private const int MaxRequestAmount = 250;
    private const int MaxScoreSubmissionAmount = 10;
    private const int RequestBlockDuration = 300;
    private const string BucketName = "score";
    private const string ScoreSubmissionBucketName = "score-submission";

    // LBP1 doesn't send any requests to this endpoint if a user enters an online user level.
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
    public Response GetUserScores(RequestContext context, GameUser user, GameDatabaseContext database, string slotType,
        int id, Token token, DataContext dataContext)
    {
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        if (level == null) return NotFound;
        
        //Get the scores from the database
        MultiLeaderboard multiLeaderboard = new(database, level, token.TokenGame);
        
        return new Response(SerializedMultiLeaderboardResponse.FromOld(multiLeaderboard, dataContext), ContentType.Xml);
    }

    [GameEndpoint("scoreboard/friends/{slotType}/{id}", HttpMethods.Post, ContentType.Xml)]
    [RateLimitSettings(RequestTimeoutDuration, MaxRequestAmount, RequestBlockDuration, BucketName)]
    [NullStatusCode(NotFound)]
    public SerializedScoreLeaderboardList? GetLevelFriendLeaderboard(RequestContext context,
        GameUser user,
        GameDatabaseContext database,
        string slotType,
        int id,
        FriendScoresRequest body,
        DataContext dataContext)
    {
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        if (level == null) return null;

        DatabaseList<ScoreWithRank>? scores = database.GetLevelTopScoresByFriends(user, level, 10, body.Type);
        return SerializedScoreLeaderboardList.FromSubmittedEnumerable(scores.Items, dataContext);
    }
    
    [GameEndpoint("scoreboard/{slotType}/{id}", ContentType.Xml, HttpMethods.Post)]
    [RateLimitSettings(RequestTimeoutDuration, MaxScoreSubmissionAmount, RequestBlockDuration, ScoreSubmissionBucketName)]
    [RequireEmailVerified]
    public Response SubmitScore(RequestContext context, GameUser user, GameServerConfig config,
        GameDatabaseContext database, string slotType, int id, SerializedScore body, Token token,
        DataContext dataContext)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;
        
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        if (level == null) return NotFound;

        // A user has to play a level in order to submit a score
        if (!database.HasUserPlayedLevel(level, user))
        {
            return Unauthorized;
        }

        // Validate the score is a non-negative amount and not above the in-game limit
        if (body.Score is < 0 or > 16_000_000)
        {
            return BadRequest;
        }
        
        // Ensure score type is valid
        // Only valid values are 1-4 players and 7 for versus
        if (body.ScoreType is (> 4 or < 1) and not 7)
        {
            return BadRequest;
        }

        GameScore score = database.SubmitScore(body, token, level);

        DatabaseList<ScoreWithRank>? scores = database.GetRankedScoresAroundScore(score, 5);
        Debug.Assert(scores != null);
        
        return new Response(SerializedScoreLeaderboardList.FromSubmittedEnumerable(scores.Items.ToArray(), dataContext), ContentType.Xml);
    }

    [GameEndpoint("topscores/{slotType}/{id}/{type}", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [RateLimitSettings(RequestTimeoutDuration, MaxRequestAmount, RequestBlockDuration, BucketName)]
    public Response GetTopScoresForLevel(RequestContext context, GameDatabaseContext database, string slotType, int id,
        int type, DataContext dataContext)
    {
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        if (level == null) return NotFound;
        
        (int skip, int count) = context.GetPageData();
        DatabaseList<GameScore>? scores = database.GetTopScoresForLevel(level, count, skip, (byte)type);

        return new Response(SerializedScoreList.FromSubmittedEnumerable(scores.Items.ToArray(), dataContext, skip), ContentType.Xml);
    }
}