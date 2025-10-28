using Refresh.Database.Models.Users;
using Refresh.Database.Models.Moderation;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Database.Models.Photos;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Playlists;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Levels.Challenges;

namespace Refresh.Database;

public partial class GameDatabaseContext // Moderation
{
    private IQueryable<ModerationAction> ModerationActionsIncluded => this.ModerationActions
        .Include(s => s.Actor)
        .Include(s => s.InvolvedUser);
    
    #region Retrieval
    
    public DatabaseList<ModerationAction> GetModerationActions(int skip, int count) {
        return new(this.ModerationActionsIncluded.OrderByDescending(a => a.Timestamp), skip, count);
    }

    public DatabaseList<ModerationAction> GetModerationActionsByActor(GameUser actor, int skip, int count) {
        return new(this.ModerationActionsIncluded
            .Where(a => a.ActorId == actor.UserId)
            .OrderByDescending(a => a.Timestamp), skip, count);
    }

    public DatabaseList<ModerationAction> GetModerationActionsForInvolvedUser(GameUser involvedUser, int skip, int count) {
        return new(this.ModerationActionsIncluded
            .Where(a => a.InvolvedUserId == involvedUser.UserId)
            .OrderByDescending(a => a.Timestamp), skip, count);
    }

    public DatabaseList<ModerationAction> GetModerationActionsForObject(string id, ModerationObjectType objectType, int skip, int count) {
        return new(this.ModerationActionsIncluded
            .Where(a => a.ModeratedObjectType == objectType && a.ModeratedObjectId == id)
            .OrderByDescending(a => a.Timestamp), skip, count);
    }

    #endregion

    #region Creation

    public ModerationAction CreateModerationAction(GameUser user, ModerationActionType actionType, GameUser actor, string description)
        => this.CreateModerationActionInternal(user.UserId.ToString(), ModerationObjectType.User, actionType, actor, user, description);
    
    public ModerationAction CreateModerationAction(GameLevel level, ModerationActionType actionType, GameUser actor, string description)
        => this.CreateModerationActionInternal(level.LevelId.ToString(), ModerationObjectType.Level, actionType, actor, level.Publisher, description);
    
    public ModerationAction CreateModerationAction(GameScore score, ModerationActionType actionType, GameUser actor, string description)
        => this.CreateModerationActionInternal(score.ScoreId.ToString(), ModerationObjectType.Score, actionType, actor, score.Publisher, description);
    
    public ModerationAction CreateModerationAction(GamePhoto photo, ModerationActionType actionType, GameUser actor, string description)
        => this.CreateModerationActionInternal(photo.PhotoId.ToString(), ModerationObjectType.Photo, actionType, actor, photo.Publisher, description);

    public ModerationAction CreateModerationAction(GameReview review, ModerationActionType actionType, GameUser actor, string description)
        => this.CreateModerationActionInternal(review.ReviewId.ToString(), ModerationObjectType.Review, actionType, actor, review.Publisher, description);

    public ModerationAction CreateModerationAction(GameLevelComment comment, ModerationActionType actionType, GameUser actor, string description)
        => this.CreateModerationActionInternal(comment.SequentialId.ToString(), ModerationObjectType.LevelComment, actionType, actor, comment.Author, description);

    public ModerationAction CreateModerationAction(GameProfileComment comment, ModerationActionType actionType, GameUser actor, string description)
        => this.CreateModerationActionInternal(comment.SequentialId.ToString(), ModerationObjectType.UserComment, actionType, actor, comment.Author, description);

    public ModerationAction CreateModerationAction(GamePlaylist playlist, ModerationActionType actionType, GameUser actor, string description)
        => this.CreateModerationActionInternal(playlist.PlaylistId.ToString(), ModerationObjectType.Playlist, actionType, actor, playlist.Publisher, description);

    public ModerationAction CreateModerationAction(GameAsset asset, ModerationActionType actionType, GameUser actor, string description)
        => this.CreateModerationActionInternal(asset.AssetHash, ModerationObjectType.Asset, actionType, actor, asset.OriginalUploader, description);

    public ModerationAction CreateModerationAction(GameChallenge challenge, ModerationActionType actionType, GameUser actor, string description)
        => this.CreateModerationActionInternal(challenge.ChallengeId.ToString(), ModerationObjectType.Challenge, actionType, actor, challenge.Publisher, description);
    
    public ModerationAction CreateModerationAction(GameChallengeScore score, ModerationActionType actionType, GameUser actor, string description)
        => this.CreateModerationActionInternal(score.ScoreId.ToString(), ModerationObjectType.Score, actionType, actor, score.Publisher, description);

    private ModerationAction CreateModerationActionInternal(string id, ModerationObjectType objectType, ModerationActionType actionType, GameUser actor, GameUser? involvedUser, string description)
    {
        ModerationAction moderationAction = new()
        {
            ModeratedObjectId = id,
            ModeratedObjectType = objectType,
            ActionType = actionType,
            Actor = actor,
            InvolvedUser = involvedUser,
            Description = description,
            Timestamp = this._time.Now,
        };

        this.ModerationActions.Add(moderationAction);
        this.SaveChanges();

        return moderationAction;
    }

    #endregion
}