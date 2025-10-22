using Refresh.Database.Models.Activity;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Relations;
using Refresh.Database.Query;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Playlists;
using Refresh.Database.Models.Levels.Challenges;
using MongoDB.Bson;
using Refresh.Database.Models.Photos;
namespace Refresh.Database;

public partial class GameDatabaseContext // ActivityWrite
{
    public Event CreateEvent(GameUser user, EventCreationParams param) 
        => this.CreateEventInternal(user.UserId, EventDataType.User, param, user);
    
    public Event CreateEvent(GameLevel level, EventCreationParams param) 
        => this.CreateEventInternal(level.LevelId, EventDataType.Level, param, level.Publisher);
    
    public Event CreateEvent(GameScore score, EventCreationParams param) 
        => this.CreateEventInternal(score.ScoreId, EventDataType.Score, param, score.Publisher);
    
    public Event CreateEvent(RateLevelRelation relation, EventCreationParams param) 
        => this.CreateEventInternal(relation.RateLevelRelationId, EventDataType.RateLevelRelation, param, relation.User);

    public Event CreateEvent(GamePhoto photo, EventCreationParams param) 
        => this.CreateEventInternal(photo.PhotoId, EventDataType.Photo, param, photo.Publisher);
    
    public Event CreateEvent(GameReview review, EventCreationParams param) 
        => this.CreateEventInternal(review.ReviewId, EventDataType.Review, param, review.Publisher);
    
    public Event CreateEvent(GameProfileComment comment, EventCreationParams param) 
        => this.CreateEventInternal(comment.SequentialId, EventDataType.UserComment, param, comment.Author);
    
    public Event CreateEvent(GameLevelComment comment, EventCreationParams param) 
        => this.CreateEventInternal(comment.SequentialId, EventDataType.LevelComment, param, comment.Author);
    
    public Event CreateEvent(GamePlaylist playlist, EventCreationParams param) 
        => this.CreateEventInternal(playlist.PlaylistId, EventDataType.Playlist, param, playlist.Publisher);
    
    public Event CreateEvent(GameChallenge challenge, EventCreationParams param) 
        => this.CreateEventInternal(challenge.ChallengeId, EventDataType.Challenge, param, challenge.Publisher);

    public Event CreateEvent(GameChallengeScore score, EventCreationParams param) 
        => this.CreateEventInternal(score.ScoreId, EventDataType.ChallengeScore, param, score.Publisher);

    // TODO: Event creation methods for Contests and Assets once the ID storing is figured out for them

    private Event CreateEventInternal(ObjectId objectId, EventDataType eventDataType, EventCreationParams param, GameUser? involvedUser = null) => 
        this.CreateEventInternal(new()
        {
            StoredObjectId = objectId,
            Timestamp = this._time.Now,
            StoredDataType = eventDataType,
            InvolvedUser = involvedUser,
            User = param.Actor,
            EventType = param.EventType,
            IsModified = param.IsModified,
            OverType = param.OverType,
            AdditionalInfo = param.AdditionalInfo,
        });
    
    private Event CreateEventInternal(int sequentialId, EventDataType eventDataType, EventCreationParams param, GameUser? involvedUser = null) => 
        this.CreateEventInternal(new()
        {
            StoredSequentialId = sequentialId,
            Timestamp = this._time.Now,
            StoredDataType = eventDataType,
            InvolvedUser = involvedUser,
            User = param.Actor,
            EventType = param.EventType,
            IsModified = param.IsModified,
            OverType = param.OverType,
            AdditionalInfo = param.AdditionalInfo,
        });

    private Event CreateEventInternal(Event e)
    {
        this.Events.Add(e);
        this.SaveChanges();
        return e;
    }
}