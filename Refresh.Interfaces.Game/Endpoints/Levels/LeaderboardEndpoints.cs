using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.RateLimit;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.Common.Time;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Configuration;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Database.Models.Pins;
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

        DatabaseList<ScoreWithRank>? scores = database.GetLevelTopScoresByFriends(user, level, 0, 10, body.Type);
        return SerializedScoreLeaderboardList.FromDatabaseList(scores, dataContext);
    }
    
    [GameEndpoint("scoreboard/{slotType}/{id}", ContentType.Xml, HttpMethods.Post)]
    [RateLimitSettings(RequestTimeoutDuration, MaxScoreSubmissionAmount, RequestBlockDuration, ScoreSubmissionBucketName)]
    [RequireEmailVerified]
    public Response SubmitScore(RequestContext context, GameUser user, GameServerConfig config,
        GameDatabaseContext database, string slotType, int id, SerializedScore body, Token token,
        DataContext dataContext)
    {
        // Try to not return any non-OK responses for LBP PSP here, since that apparently bugs the game
        bool isPSP = context.IsPSP();

        if (user.IsWriteBlocked(config))
            return isPSP ? OK : Unauthorized;
        
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        if (level == null) return isPSP ? OK : NotFound;

        // A user has to play a level in order to submit a score (unless the game is LBP PSP)
        if (!database.HasUserPlayedLevel(level, user) && !isPSP)
        {
            return Unauthorized;
        }

        // Add the uploader to the list of player GameUsers to not have to search for them in the database aswell
        List<GameUser> players = [user];

        // LBP PSP doesn't have any score limits, and checking the score type and player list isn't really useful
        // since there is no multiplayer in that game
        if (!isPSP)
        {
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

            // The score type can't be lower than the number or players
            if (body.ScoreType < body.PlayerUsernames.Count)
            {
                return BadRequest;
            }

            // Validate the player list
            if (body.PlayerUsernames.Count > 4 || !body.PlayerUsernames.Contains(user.Username))
            {
                return BadRequest;
            }

            // Normally the score type is the total of both online and local users who participated in a score.
            // If type is 7, fall back to the number of online players, to avoid showing scores "by 7 people".
            if (body.ScoreType is 7)
            {
                body.ScoreType = (byte)body.PlayerUsernames.Count;
            }

            if (body.PlayerUsernames.Count > 1)
            {
                // Only add participants who are on this server, and make sure to not unnessesarily
                // get the uploader's GameUser from the database
                body.PlayerUsernames.Remove(user.Username);

                foreach (string username in body.PlayerUsernames)
                {
                    GameUser? player = database.GetUserByUsername(username, true);
                    if (player == null) continue;

                    players.Add(player);
                }
            }
        }
        else
        {
            body.ScoreType = 1;
        }

        GameScore score = database.SubmitScore(body, token, level, players);
        DatabaseList<ScoreWithRank> scores = database.GetRankedScoresAroundScore(score, 5);

        this.AwardScoreboardPins(scores, dataContext, user, level);

        return new Response(SerializedScoreLeaderboardList.FromDatabaseList(scores, dataContext), ContentType.Xml);
    }

    /// <summary>
    /// Awards certain score submission-related pins which the game expects the server to award
    /// </summary>
    private void AwardScoreboardPins(DatabaseList<ScoreWithRank> scores, DataContext dataContext, GameUser user, GameLevel level)
    {
        dataContext.Database.EnsureLevelStatisticsCreated(level);
        int uniqueScoreCount = scores.TotalItems;

        // All pins below are only expected to be awarded if the level's leaderboard has atleast 50 scores.
        // This check also prevents dividing by 0 below.
        if (uniqueScoreCount < 50) return;

        ScoreWithRank? ownScore = scores.Items.FirstOrDefault(s => s.score.PlayerIds.Contains(user.UserId));
        if (ownScore == null) return; // Should never happen, incase it somehow does, skip this part

        // Examples for rankingInPercent:
        // - rank 20 out of 40 = 50%
        // - rank 5 out of 40 = 12.5%
        // Always rounding up will prevent users from being top 0% of a leaderboard (since 1% should be the maximum)
        // after the int cast; and for the top 25% pins, being in the top 25.001% for example technically
        // doesn't count as completing the pins' objective, since that's still greater than 25%.
        int rankingInPercent = (int)Math.Ceiling((float)ownScore.rank / uniqueScoreCount * 100);
        bool isStoryLevel = level.SlotType == GameSlotType.Story;
        bool isGameBetaBuild = dataContext.Game == TokenGame.BetaBuild;

        // Update lowest rankingInPercent of any story/user level leaderboard
        if (isStoryLevel)
        {
            dataContext.Database.UpdateUserPinProgressToLowest((long)ServerPins.TopXOfAnyStoryLevelWithOver50Scores,
                rankingInPercent, user, isGameBetaBuild);
        }
        else
        {
            dataContext.Database.UpdateUserPinProgressToLowest((long)ServerPins.TopXOfAnyCommunityLevelWithOver50Scores, 
                rankingInPercent, user, isGameBetaBuild);
        }

        // Update on how many story/user levels the user's ranking is in the top 25% (top 1/4th) of the leaderboard or below
        if (rankingInPercent > 25) return;
        
        if (isStoryLevel)
        {
            dataContext.Database.IncrementUserPinProgress((long)ServerPins.TopFourthOfXStoryLevelsWithOver50Scores, 
                1, user, isGameBetaBuild);
        }
        else
        {
            dataContext.Database.IncrementUserPinProgress((long)ServerPins.TopFourthOfXCommunityLevelsWithOver50Scores, 
                1, user, isGameBetaBuild);
        }
    }

    private (byte, DateTimeOffset?) GetScoreTypeAndMinAge(int originalType, DateTimeOffset now)
    {
        return originalType switch
        {
            5 => (0, now.AddDays(-1)), // daily versus leaderboard
            6 => (0, now.AddDays(-7)), // weekly versus leaderboard
            7 => (0, null), // all time versus leaderboard
            _ => ((byte)originalType, null),
        };
    }

    [GameEndpoint("topscores/{slotType}/{id}/{type}", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    [RateLimitSettings(RequestTimeoutDuration, MaxRequestAmount, RequestBlockDuration, BucketName)]
    public SerializedScoreList? GetTopScoresForLevel(RequestContext context, GameDatabaseContext database, string slotType, int id,
        int type, DataContext dataContext, GameUser user, IDateTimeProvider dateTimeProvider)
    {
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        if (level == null) return null;
        
        (int skip, int count) = context.GetPageData();
        (byte scoreType, DateTimeOffset? minAge) = this.GetScoreTypeAndMinAge(type, dateTimeProvider.Now);

        DatabaseScoreList? scores = database.GetTopScoresForLevel(level, count, skip, scoreType, false, minAge, user);
        return SerializedScoreList.FromDatabaseList(scores, dataContext);
    }

    [GameEndpoint("friendscores/{slotType}/{id}/{type}", ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    [RateLimitSettings(RequestTimeoutDuration, MaxRequestAmount, RequestBlockDuration, BucketName)]
    public SerializedScoreList? GetFriendTopScoresForLevel(RequestContext context, GameDatabaseContext database, string slotType, int id,
        int type, DataContext dataContext, GameUser user, IDateTimeProvider dateTimeProvider)
    {
        GameLevel? level = database.GetLevelByIdAndType(slotType, id);
        if (level == null) return null;
        
        (int skip, int count) = context.GetPageData();
        (byte scoreType, DateTimeOffset? minAge) = this.GetScoreTypeAndMinAge(type, dateTimeProvider.Now);

        DatabaseScoreList? scores = database.GetLevelTopScoresByFriends(user, level, skip, count, scoreType, minAge);
        return SerializedScoreList.FromDatabaseList(scores, dataContext);
    }
}