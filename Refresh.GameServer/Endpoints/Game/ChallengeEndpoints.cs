using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Assets;
using Refresh.GameServer.Types.Challenges.LbpHub;
using Refresh.GameServer.Types.Challenges.LbpHub.Ghost;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game;

public class ChallengeEndpoints : EndpointGroup
{
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

    /// <summary>
    /// Intended to return challenges by the specified user.
    /// Usually gets called together with the GetChallengesByUsersFriends endpoint below.
    /// The query parameter "status" indicates whether to return "active" or "expired" challenges.
    /// </summary>
    [GameEndpoint("user/{username}/challenges", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    public SerializedChallengeList? GetChallengesByUser(RequestContext context, DataContext dataContext, string username)
    {
        GameUser? user = dataContext.Database.GetUserByUsername(username);
        if (user == null) return null;

        string? status = context.QueryString.Get("status");
        DatabaseList<GameChallenge> challenges = dataContext.Database.GetChallengesByUser(user, 0, 100, status);
        
        return new SerializedChallengeList(SerializedChallenge.FromOldList(challenges.Items, dataContext).ToList());
    }

    /// <summary>
    /// Intended to return challenges by the specified user's friends.
    /// Return all challenges except those by the specified user instead, as outside of the "Past Challenges" page in the pod, 
    /// the game only ever uses this endpoint and the GetChallengesByUser endpoint above to get and display challenges, 
    /// effectively only letting the player play their own and their friends' challenges.
    /// Likely not many people play LBP Hub anyway, resulting in the number of potential challenges likely being low,
    /// and making most of these challenges unplayable on top of that wouldn't be very smart.
    /// The query parameter "status" indicates whether to return "active" or "expired" challenges.
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
            challenges = dataContext.Database.GetChallenges(0, 1000, status);
        else
            challenges = dataContext.Database.GetChallengesNotByUser(user, 0, 1000, status);

        return new SerializedChallengeList(SerializedChallenge.FromOldList(challenges.Items, dataContext).ToList());
    }

    /// <summary>
    /// Most likely intended to get the specified user's and their friend's challenges.
    /// Return all challenges instead, for the same reason described in GetChallengesByUsersFriends' summary above. 
    /// Usually this endpoint only gets called when going to "Past Challenges" in the pod.
    /// The query parameter "status" indicates whether to return "active" or "expired" challenges.
    /// </summary>
    [GameEndpoint("user/{username}/challenges/joined", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    public SerializedChallengeList? GetJoinedChallenges(RequestContext context, DataContext dataContext, string username)
    {
        string? status = context.QueryString.Get("status");

        DatabaseList<GameChallenge> challenges = dataContext.Database.GetChallenges(0, 1000, status);
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
    /// The response has to be an empty list, otherwise, after finishing a challenge, LBP Hub will hide the user's friends' (mutuals' in our case)
    /// scores returned with <see cref="GetContextualScoresForChallenge"/> in a messy way.
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
    /// Gets called when a user finishes a challenge to show a 3 scores large fragment of it's leaderboard with scores
    /// "around" the user's high score. The username of that user is sometimes empty, 
    /// therefore only use the token's user for simplicity (the game never calls the contextual leaderboard of any other user).
    /// </summary>
    /// <remarks>
    /// This endpoint is also used to get the next best score if the user's highscore for this challenge exists, but is not rank 1.
    /// Unfortunately, instead of only getting the next score's ghost asset with <see cref="ResourceEndpoints.GetResource"/> afterwards, 
    /// the game will then also try to get the ghost asset of every score in this endpoint's response, to then seemingly combine them into one asset,
    /// completely breaking ghost replay. To work around this, we block all ghost asset requests to the GetResource endpoint past the first, correct one
    /// using <see cref="ChallengeGhostRateLimitService._challengeGhostRateLimitedUsers"/>
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