using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Configuration;
using Refresh.Core.Services;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Levels.Challenges;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Types.Challenges.LbpHub;
using Refresh.Interfaces.Game.Types.Challenges.LbpHub.Ghost;
using Refresh.Interfaces.Game.Types.Lists;

namespace Refresh.Interfaces.Game.Endpoints;

public class ChallengeEndpoints : EndpointGroup
{
    // Try to have these endpoints be as fast as possible, as LBP Hub just freezes while it's waiting for responses of any challenge or
    // ChallengeGhost asset requests.

    #region Challenges

    [GameEndpoint("challenge", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response UploadChallenge(RequestContext context, DataContext dataContext, GameUser user, SerializedChallenge body, GameServerConfig config)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;

        GameLevel? level = dataContext.Database.GetLevelByIdAndType(body.Level.Type, body.Level.LevelId);
        if (level == null) 
            return NotFound;
        
        if (body.Criteria.Count < 1) 
        {
            dataContext.Logger.LogWarning(BunkumCategory.UserContent, $"Challenge by {user.Username} on level ID {level.LevelId} does not have any criteria, rejecting");
            return BadRequest;
        }

        if (body.Criteria.First().Value != 0)
            dataContext.Logger.LogWarning(BunkumCategory.UserContent, $"Challenge by {user.Username} on level ID {level.LevelId} has criterion value {body.Criteria.First().Value}, it won't be saved");

        if (body.Criteria.Count > 1)
            dataContext.Logger.LogWarning(BunkumCategory.UserContent, $"Challenge by {user.Username} on level ID {level.LevelId} has {body.Criteria.Count} criteria, only the first one will be saved");

        GameChallenge challenge = dataContext.Database.CreateChallenge(body, level, user);

        // Return a SerializedChallenge which is not body, else the game will not send the first score
        // and it's ghost asset for this challenge
        return new Response(SerializedChallenge.FromOld(challenge, dataContext), ContentType.Xml);
    }

    // The game includes a "status" query param for all 3 challenge list endpoints below to specify whether to return "active"
    // or "expired" challenges. Implementing this functionality as intended would make challenges effectively only playable for the first 3 - 7 days
    // after they're uploaded. Since not many people play LBP Hub, they'd only ever be able to play 0 challenges or atleast a small handful if they're lucky enough.
    // This is why we don't implement challenge expiration anymore.
    // The challenges returned are sorted by newest anyway, and since Hub doesn't send any pagination params for challenges, we're only returning the newest 100 anyway.

    /// <summary>
    /// Returns challenges by the specified user. Gets called alongside the endpoint below
    /// </summary>
    [GameEndpoint("user/{username}/challenges", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    public SerializedChallengeList? GetChallengesByUser(RequestContext context, DataContext dataContext, string username)
    {
        GameUser? user = dataContext.Database.GetUserByUsername(username);
        if (user == null) return null;

        string? status = context.QueryString.Get("status");
        DatabaseList<GameChallenge> challenges = dataContext.Database.GetChallengesByUser(user, 0, 100);
        
        return new SerializedChallengeList(SerializedChallenge.FromOldList(challenges.Items, dataContext).ToList());
    }

    /// <summary>
    /// Intended to return challenges by the specified user's friends. Gets called alongside the endpoint above.
    /// Return all but this user's challenges instead, as not many people play LBP Hub and only making a fraction of an already
    /// very small count of challenges playable doesn't make any sense here.
    /// </summary>
    [GameEndpoint("user/{username}/friends/challenges", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    public SerializedChallengeList? GetChallengesByUsersFriends(RequestContext context, DataContext dataContext, string username)
    {
        GameUser? user = dataContext.Database.GetUserByUsername(username);
        string? status = context.QueryString.Get("status");

        DatabaseList<GameChallenge> challenges;
        if (user == null)
            challenges = dataContext.Database.GetNewestChallenges(0, 100);
        else
            challenges = dataContext.Database.GetChallengesNotByUser(user, 0, 100);

        return new SerializedChallengeList(SerializedChallenge.FromOldList(challenges.Items, dataContext).ToList());
    }

    /// <summary>
    /// Get's both the specified user's and their friend's challenges. Only gets called in the Past Challenges page while both
    /// endpoints above get called everywhere else. Don't know why only Past Challenges uses this endpoint, considering the "status"
    /// query param already specifies whether to return active or expired challenges for all 3 endpoints.
    /// </summary>
    [GameEndpoint("user/{username}/challenges/joined", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    public SerializedChallengeList? GetJoinedChallenges(RequestContext context, DataContext dataContext, string username)
    {
        string? status = context.QueryString.Get("status");

        DatabaseList<GameChallenge> challenges = dataContext.Database.GetNewestChallenges(0, 100);
        return new SerializedChallengeList(SerializedChallenge.FromOldList(challenges.Items, dataContext).ToList());
    }

    #endregion

    #region Scores

    /// <summary>
    /// Gets called when submitting a challenge score after either beating an opponent's challenge score or right after uploading a challenge.
    /// Usually this endpoint only gets called after the game is done uploading the ChallengeGhost asset for this score.
    /// </summary>
    [GameEndpoint("challenge/{challengeId}/scoreboard", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response SubmitChallengeScore(RequestContext context, DataContext dataContext, GameUser user,
        SerializedChallengeAttempt body, int challengeId, ChallengeGhostRateLimitService ghostService,
        GameServerConfig config)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;

        ghostService.RemoveUserFromChallengeGhostRateLimit(user.UserId);

        GameChallenge? challenge = dataContext.Database.GetChallengeById(challengeId);
        if (challenge == null) return NotFound;

        GameAsset? ghostAsset = dataContext.Database.GetAssetFromHash(body.GhostHash);

        // If there is no GameAsset in the database with the score's GhostHash, or the referred asset is not a ChallengeGhost for some reason,
        // reject the score.
        if (ghostAsset == null || ghostAsset.AssetType != GameAssetType.ChallengeGhost)
        {
            dataContext.Database.AddErrorNotification
            (
                "Challenge Score upload failed", 
                $"Your score for '{challenge.Name}' in '{challenge.Level.Title}' "
                +"couldn't be submitted because the ghost data was missing.",
                user
            );
            dataContext.Logger.LogDebug(BunkumCategory.UserContent, $"Ghost asset with hash {body.GhostHash} was not found or is not a ChallengeGhost");
            return BadRequest;
        }

        SerializedChallengeGhost? serializedGhost = SerializedChallengeGhost.FromDataStore(body.GhostHash, dataContext.DataStore, dataContext.Logger);
        bool isFirstScore = !dataContext.Database.DoesChallengeHaveScores(challenge);
        
        // If the ghost asset for this score is null or invalid, reject the score
        if (serializedGhost == null || !SerializedChallengeGhost.IsGhostDataValid(serializedGhost, challenge, isFirstScore))
        {
            dataContext.Database.AddErrorNotification
            (
                "Challenge Score upload failed", 
                $"Your score for '{challenge.Name}' in '{challenge.Level.Title}' "
                +"couldn't be submitted because the ghost data was corrupt. "
                +"Try to submit another score!",
                user
            );
            dataContext.Logger.LogDebug(BunkumCategory.UserContent, $"Ghost asset with hash {body.GhostHash} is corrupt");
            return BadRequest;
        }

        // The time (in whole seconds) it took the player to achieve this score, independent of challenge criteria
        long time = serializedGhost.Checkpoints.Last().Time - serializedGhost.Checkpoints.First().Time;
        
        dataContext.Database.CreateChallengeScore(body, challenge, user, time);
        return OK;
    }

    /// <param name="username">The username of the user to return the high score by. Can be empty sometimes, if it is, return NotFound.</param>
    [GameEndpoint("challenge/{challengeId}/scoreboard/{username}", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    public SerializedChallengeScore? GetUsersHighScoreForChallenge(RequestContext context, DataContext dataContext, GameUser user, int challengeId, string username, ChallengeGhostRateLimitService ghostService) 
    {
        ghostService.RemoveUserFromChallengeGhostRateLimit(user.UserId);

        if (string.IsNullOrEmpty(username)) return null;

        GameChallenge? challenge = dataContext.Database.GetChallengeById(challengeId);
        if (challenge == null) return null;
            
        GameUser? requestedUser = dataContext.Database.GetUserByUsername(username);
        if (requestedUser == null) return null;
        
        GameChallengeScoreWithRank? score = dataContext.Database.GetRankedChallengeHighScoreByUser(challenge, requestedUser);
        return SerializedChallengeScore.FromOld(score);
    }

    [GameEndpoint("challenge/{challengeId}/scoreboard", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    public SerializedChallengeScoreList? GetScoresForChallenge(RequestContext context, DataContext dataContext, GameUser user, int challengeId, ChallengeGhostRateLimitService ghostService)
    {
        ghostService.RemoveUserFromChallengeGhostRateLimit(user.UserId);

        GameChallenge? challenge = dataContext.Database.GetChallengeById(challengeId);
        if (challenge == null) return null;

        DatabaseList<GameChallengeScoreWithRank> scores = dataContext.Database.GetRankedChallengeHighScores(challenge, 0, 1000);
        return new SerializedChallengeScoreList(SerializedChallengeScore.FromOldList(scores.Items));
    }

    /// <summary>
    /// Intended to return the scores of a challenge by a user's friends, specified by that user's username.
    /// </summary>
    /// <remarks>
    /// The response has to be an empty list, else LBP Hub will hide all scores included here from the fragment returned by 
    /// <see cref="GetContextualScoresForChallenge"/> in a messy way.
    /// </remarks>
    [GameEndpoint("challenge/{challengeId}/scoreboard/{username}/friends", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    public SerializedChallengeScoreList? GetScoresByUsersFriendsForChallenge(RequestContext context, DataContext dataContext, GameUser user, int challengeId, ChallengeGhostRateLimitService ghostService)
    {
        ghostService.RemoveUserFromChallengeGhostRateLimit(user.UserId);

        GameChallenge? challenge = dataContext.Database.GetChallengeById(challengeId);
        if (challenge == null) return null;

        return new SerializedChallengeScoreList();
    }

    /// <summary>
    /// Called to get a 3 scores large fragment of a challenge's leaderboard, with the specified user's score being preferrably in the middle.
    /// This is used to both show where the user's current high score is in the leaderboard after playing a challenge, and to find out the next score to beat
    /// if the user's own score isn't rank 1 already.
    /// </summary>
    /// <remarks>
    /// The latter also leads to an annoying bug where the game will first download the ghost asset of that next best score, but then also those of
    /// every score in this endpoint's response (in other words, it'll try downloading the next best score's asset again and that of the other scores).
    /// It'll then seemingly somehow combine them all into one asset and try to play it back during the challenge, breaking the replay due to 
    /// way to large coordinates. We work around this by implementing a rate limit for ghost assets in <see cref="ResourceEndpoints.GetResource"/>
    /// using <see cref="ChallengeGhostRateLimitService._challengeGhostRateLimitedUsers"/>, so that Hub only gets the first, correct asset.
    /// </remarks>
    [GameEndpoint("challenge/{challengeId}/scoreboard/{username}/contextual", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    public SerializedChallengeScoreList? GetContextualScoresForChallenge(RequestContext context, DataContext dataContext, GameUser user, int challengeId, ChallengeGhostRateLimitService ghostService) 
    {
        ghostService.RemoveUserFromChallengeGhostRateLimit(user.UserId);

        GameChallenge? challenge = dataContext.Database.GetChallengeById(challengeId);
        if (challenge == null) return null;

        GameChallengeScoreWithRank? highScore = dataContext.Database.GetRankedChallengeHighScoreByUser(challenge, user);
        DatabaseList<GameChallengeScoreWithRank> scores = highScore == null  
            // The user does not have a score on this challenge, return the lowest 3 scores of it
            ? dataContext.Database.GetLowestRankedChallengeHighScores(challenge, 0, 3)
            // The user does have a score on this challenge, return the scores around that score
            : dataContext.Database.GetRankedHighScoresAroundChallengeScore(highScore, 3);

        return new SerializedChallengeScoreList(SerializedChallengeScore.FromOldList(scores.Items));
    }

    #endregion

    #region Story Challenges
    // TODO: Implement story challenges

    // developer-challenges/scores?ids=1&ids=2&ids=3&ids=4
    [GameEndpoint("developer-challenges/scores", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public Response GetDeveloperChallengeScores(RequestContext context, DataContext dataContext)
        => NotImplemented; // LBP Hub handles this response well

    // developer-challenges/3/scores
    [GameEndpoint("developer-challenges/{challengeId}/scores", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response SubmitDeveloperChallengeScore(RequestContext context, DataContext dataContext, GameUser user, int challengeId, string body)
        => NotImplemented; // LBP Hub handles this response well

    #endregion
}