using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Endpoints.Debugging;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.Common.Constants;
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
    // Try to have these endpoints be as fast as possible, as LBP Hub just freezes while it's waiting for any challenge or
    // ChallengeGhost asset request responses.

    #region Challenges

    [GameEndpoint("challenge", HttpMethods.Post, ContentType.Xml)]
    [RequireEmailVerified]
    public Response UploadChallenge(RequestContext context, DataContext dataContext, GameUser user, SerializedChallenge body, GameServerConfig config)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;

        // Using the level specified in the request body is unreliable, since Hub allows users to create challenges in moon levels, in which case their
        // slot type is wrongly set to user instead of local, and the ID is that of the moon slot. This would cause the server to attribute these challenges
        // to wrong levels on top of already having them be completely unplayable due to being for moon levels anyway, or potentially cause a crash if the level
        // the server attributes the challenge to happens to have a checkpoint of the same Uid as the challenge's start checkpoint, but none matching the
        // finish checkpoint. Since users can only ever create challenges in levels they're currently playing, and LBP Hub also updates the user's room data 
        // with /match, we can simply use room data to find out the correct level.
        // If showing online users is disabled, we won't be able to get the user's room even if they're online, so fall back to using the request body in that case.
        // Don't want challenge uploading to no longer work if showing online users is disabled.
        GameLevel? level;
        GameRoom? room = dataContext.Match.RoomAccessor.GetRoomByUser(user, dataContext.Platform, dataContext.Game);

        if (room != null)
        {
            RoomSlotType roomSlotType = room.LevelType;
            if (roomSlotType != RoomSlotType.Story && roomSlotType != RoomSlotType.Online && roomSlotType != RoomSlotType.DLC)
            {
                dataContext.Logger.LogWarning(BunkumCategory.UserContent, $"Rejecting {user}'s challenge due to actually being for slot type {roomSlotType}");
                dataContext.Database.AddErrorNotification
                (
                    "Challenge upload failed", 
                    $"Your challenge '{body.Name}' couldn't be uploaded because it was not a story, DLC or community level challenge.",
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

        // Trim name
        if (body.Name.Length > UgcLimits.TitleLimit) 
            body.Name = body.Name[..UgcLimits.TitleLimit];

        GameChallenge challenge = dataContext.Database.CreateChallenge(body, level, user);

        // Return a SerializedChallenge which is not body, else the game will not send the first score
        // and it's ghost asset for this challenge
        return new Response(SerializedChallenge.FromOld(challenge, dataContext), ContentType.Xml);
    }

    // The game includes a "status" query param for all 3 challenge list endpoints below to specify whether to return "active"
    // or "expired" challenges. Implementing this functionality as intended would make challenges only playable for the first 3 - 7 days
    // after they're uploaded. Since not many people play LBP Hub, they'd only ever be able to play 0 challenges or atleast a small handful if they're lucky enough.
    // This is why we don't implement challenge expiration anymore, and instead just return them sorted by newest.
    //
    // Also, we can safely ignore that the game expects us to only return the user's own and their friends' challenges for the same reason mentioned above
    // (not many players => not many challenges to begin with), and just return all challenges regardless of publisher.
    //
    // Also, the status param is redundant anyway because Hub only uses /joined for the Past Challenges pod menu page (with status only ever set to expired)
    // and both /user and /friends together anywhere else (with status only ever set to active). Since Hub also doesn't differenciate between challenges 
    // returned by /user and /friends, we can optimize these endpoints by having /user return all challenges and /friends return none.
    // Also have /joined return just the user's challenges since the Past Challenges page doesn't have any purpose anymore.

    [GameEndpoint("user/{username}/challenges", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    public Response GetChallengesByUser(RequestContext context, GameUser user, DataContext dataContext)
    {
        GameRoom? room = dataContext.Match.RoomAccessor.GetRoomByUser(user, dataContext.Platform, dataContext.Game);
        DatabaseList<GameChallenge> challenges;

        // If the user is in a level, try to only return the challenges of that level, to make responses smaller
        if (room != null)
        {
            GameSlotType slotType = room.LevelType.ToGameSlotType();
            switch (slotType)
            {
                case GameSlotType.User:
                case GameSlotType.Story:
                    GameLevel? level = dataContext.Database.GetLevelByIdAndType(slotType, room.LevelId);

                    // Won't be able to play any challenges if level doesn't exist anyway
                    if (level == null)
                    {
                        return NotFound;
                    }

                    challenges = dataContext.Database.GetChallengesForLevel(level, 0, 100);
                    break;
                case GameSlotType.Pod:
                    challenges = dataContext.Database.GetNewestChallenges(0, 100);
                    break;
                case GameSlotType.Moon:
                    // No challenges for moon levels
                    return BadRequest;
                default:
                    context.Logger.LogWarning(BunkumCategory.UserContent, $"Challenges requested for invalid slot type: {slotType}");
                    return BadRequest;
            }
        }
        else
        {
            challenges = dataContext.Database.GetNewestChallenges(0, 100);
        }

        SerializedChallengeList response = new(SerializedChallenge.FromOldList(challenges.Items, dataContext).ToList());
        return new Response(response, ContentType.Xml);
    }

    [GameEndpoint("user/{username}/friends/challenges", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    public SerializedChallengeList? GetChallengesByFriends(RequestContext context)
        => new([]);

    [GameEndpoint("user/{username}/challenges/joined", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    public Response GetJoinedChallenges(RequestContext context, GameUser user, DataContext dataContext)
    {
        // Ignore username since this is only ever called for the calling user. 
        // Also this endpoint is never called while in a level, so skip filtering by level.
        DatabaseList<GameChallenge> challenges = dataContext.Database.GetChallengesByUser(user, 0, 100);
        SerializedChallengeList response = new(SerializedChallenge.FromOldList(challenges.Items, dataContext).ToList());

        return new Response(response, ContentType.Xml);
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

        ghostService.RemoveUserFromRateLimit(user.UserId);

        GameChallenge? challenge = dataContext.Database.GetChallengeById(challengeId);
        if (challenge == null) return NotFound;

        if (!dataContext.DataStore.ExistsInStore(body.GhostHash))
        {
            dataContext.Database.AddErrorNotification
            (
                "Challenge Score upload failed", 
                $"Your score for '{challenge.Name}' in '{challenge.Level.Title}' "
                +"couldn't be submitted because the ghost data was missing.",
                user
            );
            dataContext.Logger.LogDebug(BunkumCategory.UserContent, $"Ghost asset with hash {body.GhostHash} was not found in data store");
            return BadRequest;
        }

        SerializedChallengeGhost? serializedGhost = SerializedChallengeGhost.FromDataStore(body.GhostHash, dataContext.DataStore, dataContext.Logger);
        bool isFirstScore = !dataContext.Database.DoesChallengeHaveScores(challenge);
        
        // If the ghost asset for this score is null or invalid, reject the score
        if (serializedGhost == null || !SerializedChallengeGhost.IsGhostDataValid(serializedGhost, challenge, isFirstScore, dataContext.Logger))
        {
            dataContext.Database.AddErrorNotification
            (
                "Challenge Score upload failed",
                $"Your score for '{challenge.Name}' in '{challenge.Level.Title}' "
                +"couldn't be submitted because the ghost data was corrupt. "
                +"Try to submit another score!",
                user
            );
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
        ghostService.RemoveUserFromRateLimit(user.UserId);

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
        ghostService.RemoveUserFromRateLimit(user.UserId);

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
        // No need to reset ghost asset rate limit, the game already sends requests to enough other score endpoints at this point
        => new([]);

    /// <summary>
    /// Called to get a 3 scores large fragment of a challenge's leaderboard, with the specified user's highscore being preferrably in the middle.
    /// This is used to both show where the user's current high score is in the leaderboard after playing a challenge, and to find out the next score to beat
    /// if the user's own score isn't rank 1 already.
    /// </summary>
    [GameEndpoint("challenge/{challengeId}/scoreboard/{username}/contextual", HttpMethods.Get, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(NotFound)]
    public SerializedChallengeScoreList? GetContextualScoresForChallenge(RequestContext context, DataContext dataContext, GameUser user, int challengeId, ChallengeGhostRateLimitService ghostService) 
    {
        ghostService.RemoveUserFromRateLimit(user.UserId);

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