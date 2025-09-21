using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Endpoints.Debugging;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Configuration;
using Refresh.Core.Services;
using Refresh.Core.Types.Data;
using Refresh.Core.Types.Matching;
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

        // Using the level specified in the request body is unreliable, since Hub allows users to create challenges in moon levels, in which case their
        // slot type is wrongly set to user instead of local, and the ID is that of the moon slot. This would cause the server to attribute these challenges
        // to wrong levels on top of already having them be completely unplayable due to being for moon levels anyway. Might also cause a crash if the level
        // the server attributes the challenge to happens to have a checkpoint of the same Uid as the challenge's start checkpoint, but none matching the
        // finish checkpoint. Since users can only ever create challenges in levels they're currently playing, and LBP Hub uses the /match endpoint seemingly
        // just like LBP2, we can simply use room data to find out the correct level.
        // If showing online users is disabled, we won't be able to get the user's room even if they're online, so fall back to using the request body in that case.
        // Don't want challenge uploading to no longer work if showing online users is disabled.
        GameLevel? level;
        GameRoom? room = dataContext.Match.RoomAccessor.GetRoomByUser(user, dataContext.Platform, dataContext.Game);

        if (room != null)
        {
            RoomSlotType roomSlotType = room.LevelType;
            if (roomSlotType != RoomSlotType.Story && roomSlotType != RoomSlotType.Online && roomSlotType != RoomSlotType.DLC)
            {
                dataContext.Database.AddErrorNotification
                (
                    "Challenge upload failed", 
                    $"Your challenge '{body.Name}' counldn't be uploaded because it was not a story, DLC or community level challenge.",
                    user
                );
                return BadRequest;
            }

            level = dataContext.Database.GetLevelByIdAndType(roomSlotType.ToGameSlotType(), body.Level.LevelId);
        }
        else
        {
            level = dataContext.Database.GetLevelByIdAndType(body.Level.Type, body.Level.LevelId);
        }
        
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
    
    private GameLevel? FromRoom(GameUser user, DataContext dataContext)
    {
        GameLevel? level;
        GameRoom? room = dataContext.Match.RoomAccessor.GetRoomByUser(user, dataContext.Platform, dataContext.Game);

        if (room != null)
        {
            GameSlotType slotType = room.LevelType.ToGameSlotType();
            switch (slotType)
            {
                case GameSlotType.User:
                case GameSlotType.Story:
                    level = dataContext.Database.GetLevelByIdAndType(slotType, room.LevelId);

                    // Won't be able to play any challenges if level doesn't exist anyway
                    if (level == null)
                    {
                        throw new InvalidOperationException("Challenges requested for non-existent level, this is supposed to be caught by the caller.");
                    }
                    return level;
                case GameSlotType.Pod:
                    // Do nothing, allow all challenges so the game could show them in the pod menu.
                    return null;
                case GameSlotType.Moon:
                    // No challenges for moon levels
                    throw new InvalidOperationException("Challenges requested for moon slot type, this is supposed to be caught by the caller.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(slotType), slotType, "Challenges requested for invalid slot type");
            }
        }

        return null;
    }

    /// <summary>
    /// Supposed to return challenges by the specified user. Always called alongside the friends endpoint below. Since the game does nothing to differenciate
    /// between this endpoint's response and /friends' response, and always calls this and /friends together,
    /// it's faster to return nothing here and everything with /friends, since we don't have to do any user lookups and save one challenge query this way.
    /// </summary>
    [GameEndpoint("user/{username}/challenges", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    public SerializedChallengeList? GetChallengesByUser(RequestContext context)
        => new([]);

    /// <summary>
    /// /friends is supposed to get the specified user's friends' challenges, and /joined is supposed to get both the user's and
    /// their friends' challenges. It's really counter-productive to only limit users to be able to play a small fraction of an already very small selection
    /// (since not many people play Hub), so just return challenges by anyone here.
    /// The reason why /friends and /joined are handelled the same is explained in the /user endpoint's summary. 
    /// Don't know why /joined is only ever called while in the Past Challenges menu and both /user + /friends everywhere else, 
    /// considering the "status" query param already specifies whether to return active or expired challenges for all 3 endpoints.
    /// </summary>
    [GameEndpoint("user/{username}/challenges/joined", HttpMethods.Get, ContentType.Xml)]
    [GameEndpoint("user/{username}/friends/challenges", HttpMethods.Get, ContentType.Xml)]
    [DebugResponseBody]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    public SerializedChallengeList? GetJoinedChallenges(RequestContext context, GameUser user, DataContext dataContext)
    {
        GameLevel? level;
        try
        {
            level = this.FromRoom(user, dataContext);
        }
        catch (InvalidOperationException)
        {
            // Ignore and just return
            return null;
        }

        DatabaseList<GameChallenge> challenges = dataContext.Database.GetNewestChallenges(0, 100, level);
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
    public SerializedChallengeScoreList? GetScoresByUsersFriendsForChallenge(RequestContext context, int challengeId)
        // No need to reset ghost asset rate limit, the game already sends requests to enough other score endpoints at that point
        => new([]);

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