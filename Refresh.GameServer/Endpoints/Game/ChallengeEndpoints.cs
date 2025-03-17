using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Database;
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
    [NullStatusCode(NotFound)]
    public Response UploadChallenge(RequestContext context, DataContext dataContext, GameUser user, SerializedChallenge body)
    {
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
        IEnumerable<GameChallenge> challenges = dataContext.Database.GetChallengesByUser(user, status);
        
        return new SerializedChallengeList(SerializedChallenge.FromOldList(challenges, dataContext).ToList());
    }

    /// <summary>
    /// Intended to return challenges by the specified user's friends,
    /// but since not that many people play LBP hub (considering its higher barrier to entry),
    /// it makes more sense to just return all challenges for this endpoint instead.
    /// Exclude challenges by the specified user (if found by the username in the route parameters) to not show duplicates in-game, 
    /// since this endpoint usually gets called together with the GetChallengesByUser endpoint above.
    /// The query parameter "status" indicates whether to return "active" or "expired" challenges.
    /// </summary>
    [GameEndpoint("user/{username}/friends/challenges", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    public SerializedChallengeList? GetChallengesByUsersFriends(RequestContext context, DataContext dataContext, string username)
    {
        GameUser? user = dataContext.Database.GetUserByUsername(username);
        string? status = context.QueryString.Get("status");

        IEnumerable<GameChallenge> challenges;
        if (user == null)
            challenges = dataContext.Database.GetChallenges(status);
        else
            challenges = dataContext.Database.GetChallengesNotByUser(user, status);

        return new SerializedChallengeList(SerializedChallenge.FromOldList(challenges, dataContext).ToList());
    }

    /// <summary>
    /// Most likely intended to get the specified user's and their friend's challenges.
    /// Return all challenges instead, for the same reason described in GetChallengesByUsersFriends' summary. 
    /// Usually this endpoint only gets called when going to "Past Challenges" in the pod.
    /// The query parameter "status" indicates whether to return "active" or "expired" challenges.
    /// </summary>
    [GameEndpoint("user/{username}/challenges/joined", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    public SerializedChallengeList? GetJoinedChallenges(RequestContext context, DataContext dataContext, string username)
    {
        string? status = context.QueryString.Get("status");
        IEnumerable<GameChallenge> challenges = dataContext.Database.GetChallenges(status)
            .OrderByDescending(c => c.ExpirationDate);;

        return new SerializedChallengeList(SerializedChallenge.FromOldList(challenges, dataContext).ToList());
    }

    #endregion

    #region Scores

    /// <summary>
    /// Gets called when submitting a challenge score after either beating an opponent's challenge score or right after uploading a challenge.
    /// Usually this endpoint only gets called after the game is done uploading the ChallengeGhost asset for this score.
    /// </summary>
    [GameEndpoint("challenge/{challengeId}/scoreboard", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response SubmitChallengeScore(RequestContext context, DataContext dataContext, GameUser user, SerializedChallengeAttempt body, int challengeId)
    {
        GameChallenge? challenge = dataContext.Database.GetChallengeById(challengeId);
        if (challenge == null) return NotFound;

        GameAsset? ghostAsset = dataContext.Database.GetAssetFromHash(body.GhostHash);

        // If there is no GameAsset in the database with the score's GhostHash, or the referred asset is not a ChallengeGhost for some reason,
        // reject the score.
        if (ghostAsset == null || ghostAsset.AssetType != GameAssetType.ChallengeGhost)
        {
            dataContext.Database.AddErrorNotification(
                "Challenge Score upload failed", 
                $"Your score for challenge '{challenge.Name}' in level '{challenge.Level.Title}' "
                +"couldn't be uploaded because it's ghost data was missing.",
                user
            );
            dataContext.Logger.LogDebug(BunkumCategory.UserContent, $"Ghost asset with hash {body.GhostHash} was not found or is not a ChallengeGhost");
            return BadRequest;
        }

        SerializedChallengeGhost? serializedGhost = SerializedChallengeGhost.GetSerializedChallengeGhostFromDataStore(body.GhostHash, dataContext.DataStore);
        bool isFirstScore = !dataContext.Database.DoesChallengeHaveScores(challenge);
        
        // If the ghost asset for this score is null or invalid, reject the score
        if (!SerializedChallengeGhost.IsGhostDataValid(serializedGhost, challenge, isFirstScore))
        {
            dataContext.Database.AddErrorNotification(
                "Challenge Score upload failed", 
                $"Your score for challenge '{challenge.Name}' in level '{challenge.Level.Title}' "
                +"couldn't be uploaded because it's ghost data was corrupt. "
                +"Try to submit another score!",
                user
            );
            dataContext.Logger.LogDebug(BunkumCategory.UserContent, $"Ghost asset with hash {body.GhostHash} is corrupt");
            return BadRequest;
        }

        dataContext.Database.CreateChallengeScore(body, challenge, user, serializedGhost!.Checkpoints.Last().Time - serializedGhost!.Checkpoints.First().Time);
        return OK;
    }

    /// <summary>
    /// Intended to return the high score of a user for a challenge. Return the challenge's first score if
    /// the player hasn't cleared this challenge yet, otherwise the requested user's high score.
    /// </summary>
    // NOTE: When a player is about to play a challenge in a level and LBP Hub requests for a user's high score, if you send a score which is not actually
    //       the high score for that user, the game will send one additional request to this endpoint and another one to GetContextualScoresForChallenge,
    //       load the score we returned fine, but it will bug out and break the ghost asset's path replay
    [GameEndpoint("challenge/{challengeId}/scoreboard/{username}", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    public SerializedChallengeScore? GetUsersHighScoreForChallenge(RequestContext context, DataContext dataContext, GameUser requestingUser, int challengeId, string username) 
    {
        GameChallenge? challenge = dataContext.Database.GetChallengeById(challengeId);
        if (challenge == null) return null;

        GameUser? requestedUser = dataContext.Database.GetUserByUsername(username);
        if (requestedUser == null) return null;

        return SerializedChallengeScore.FromOld(dataContext.Database.GetRankedHighScoreByUserForChallenge(challenge, requestedUser));
    }

    /// <summary>
    /// This endpoint returns the scores of a challenge. Normally the game takes care of assigning rank numbers to scores.
    /// LBP Hub does not send any pagination parameters, but it does only ever show the first 10 scores.
    /// </summary>
    [GameEndpoint("challenge/{challengeId}/scoreboard/", HttpMethods.Get, ContentType.Xml)]  // Called in a level when playing a challenge
    [GameEndpoint("challenge/{challengeId}/scoreboard", HttpMethods.Get, ContentType.Xml)]  // Called in the pod menu when viewing a challenge
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    public SerializedChallengeScoreList? GetScoresForChallenge(RequestContext context, DataContext dataContext, GameUser user, int challengeId)
    {
        GameChallenge? challenge = dataContext.Database.GetChallengeById(challengeId);
        if (challenge == null) return null;

        // Get and return the high scores (plus first score) of the challenge
        IEnumerable<GameChallengeScoreWithRank> scores = dataContext.Database.GetRankedHighScoresForChallenge(challenge);
        return new SerializedChallengeScoreList(SerializedChallengeScore.FromOldList(scores).ToList());
    }

    /// <summary>
    /// Intended to return the scores of a challenge by a user's friends, specified by that user's username.
    /// Return the scores by the requesting user's mutuals instead for privacy reasons.
    /// LBP Hub does not send any pagination parameters, but it does only ever show the first 10 scores.
    /// </summary>
    [GameEndpoint("challenge/{challengeId}/scoreboard/{username}/friends", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotImplemented)]
    public SerializedChallengeScoreList? GetScoresByUsersFriendsForChallenge(RequestContext context, DataContext dataContext, GameUser user, int challengeId)
    {
        GameChallenge? challenge = dataContext.Database.GetChallengeById(challengeId);
        if (challenge == null) return null;

        IEnumerable<GameChallengeScoreWithRank> scores = dataContext.Database.GetRankedHighScoresByUsersMutualsForChallenge(challenge, user);
        return new SerializedChallengeScoreList(SerializedChallengeScore.FromOldList(scores).ToList());
    }

    /// <summary>
    /// Gets called when a user finishes a challenge to show a 3 scores large fragment of it's leaderboard with the user's highscore preferrably being in
    /// the middle. Unlike in most other leaderboards, the game actually shows the score's ranks returned here.
    /// </summary>
    [GameEndpoint("challenge/{challengeId}/scoreboard/{username}/contextual", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    public SerializedChallengeScoreList? GetContextualScoresForChallenge(RequestContext context, DataContext dataContext, GameUser user, int challengeId, string username) 
    {
        GameUser? requestedUser = dataContext.Database.GetUserByUsername(username);
        if (requestedUser == null) return null;

        GameChallenge? challenge = dataContext.Database.GetChallengeById(challengeId);
        if (challenge == null) return null;

        GameChallengeScoreWithRank? highScore = dataContext.Database.GetRankedHighScoreByUserForChallenge(challenge, user);
        if (highScore == null) return null;

        DatabaseList<GameChallengeScoreWithRank> rankedScores = dataContext.Database.GetRankedHighScoresAroundChallengeScore(highScore, 3);
        return new SerializedChallengeScoreList(SerializedChallengeScore.FromOldList(rankedScores.Items));
    }

    /// <summary>
    /// Gets called together with the other GetContextualScoresForChallenge endpoint above, but it doesn't actually do anything in-game.
    /// Stubbed to always return OK.
    /// </summary>
    [GameEndpoint("challenge/{challengeId}/scoreboard//contextual" /*typo is intentional*/, HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public Response GetContextualScoresForChallenge(RequestContext context, DataContext dataContext, GameUser user, int challengeId) 
        => OK;

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