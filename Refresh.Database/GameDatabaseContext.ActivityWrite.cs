using Refresh.Database.Models.Activity;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Photos;
using Refresh.Database.Models.Relations;
namespace Refresh.Database;

public partial class GameDatabaseContext // ActivityWrite
{
    /// <summary>
    /// Creates a new LevelUpload event from a <see cref='GameLevel'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelUploadEvent(GameUser userFrom, GameLevel level)
    {
        Event e = new()
        {
            EventType = EventType.LevelUpload,
            StoredDataType = EventDataType.Level,
            Timestamp = this._time.Now,
            User = userFrom, // this is the container
            StoredSequentialId = level.LevelId, // part of the container
        };

        this.Write(() => this.Events.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new LevelFavourite event from a <see cref='GameLevel'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelFavouriteEvent(GameUser userFrom, GameLevel level)
    {
        Event e = new()
        {
            EventType = EventType.LevelFavourite,
            StoredDataType = EventDataType.Level,
            Timestamp = this._time.Now,
            User = userFrom,
            StoredSequentialId = level.LevelId,
        };

        this.Write(() => this.Events.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new LevelUnfavourite event from a <see cref='GameLevel'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelUnfavouriteEvent(GameUser userFrom, GameLevel level)
    {
        Event e = new()
        {
            EventType = EventType.LevelUnfavourite,
            StoredDataType = EventDataType.Level,
            Timestamp = this._time.Now,
            User = userFrom,
            StoredSequentialId = level.LevelId,
        };

        this.Write(() => this.Events.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new UserFavourite event from a <see cref='GameUser'/>, and adds it to the event list.
    /// </summary>
    public Event CreateUserFavouriteEvent(GameUser userFrom, GameUser user)
    {
        Event e = new()
        {
            EventType = EventType.UserFavourite,
            StoredDataType = EventDataType.User,
            Timestamp = this._time.Now,
            User = user,
            StoredObjectId = userFrom.UserId,
        };

        this.Write(() => this.Events.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new UserUnfavourite event from a <see cref='GameUser'/>, and adds it to the event list.
    /// </summary>
    public Event CreateUserUnfavouriteEvent(GameUser userFrom, GameUser user)
    {
        Event e = new()
        {
            EventType = EventType.UserUnfavourite,
            StoredDataType = EventDataType.User,
            Timestamp = this._time.Now,
            User = userFrom,
            StoredObjectId = user.UserId,
        };

        this.Write(() => this.Events.Add(e));
        return e;
    }
    
    /// <summary>
    /// Creates a new UserPostComment event from a <see cref='GameUser'/>, and adds it to the event list.
    /// </summary>
    public Event CreateUserPostCommentEvent(GameUser userFrom, GameUser user)
    {
        Event e = new()
        {
            EventType = EventType.UserPostComment,
            StoredDataType = EventDataType.User,
            Timestamp = this._time.Now,
            User = userFrom,
            StoredObjectId = user.UserId,
        };

        this.Write(() => this.Events.Add(e));
        return e;
    }
    
    /// <summary>
    /// Creates a new LevelPostComment event from a <see cref='GameLevel'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelPostCommentEvent(GameUser userFrom, GameLevel level)
    {
        Event e = new()
        {
            EventType = EventType.LevelPostComment,
            StoredDataType = EventDataType.Level,
            Timestamp = this._time.Now,
            User = userFrom,
            StoredSequentialId = level.LevelId,
        };

        this.Write(() => this.Events.Add(e));
        return e;
    }
    
        
    /// <summary>
    /// Creates a new LevelDeleteComment event from a <see cref='GameLevel'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelDeleteCommentEvent(GameUser userFrom, GameLevel level)
    {
        Event e = new()
        {
            EventType = EventType.LevelDeleteComment,
            StoredDataType = EventDataType.Level,
            Timestamp = this._time.Now,
            User = userFrom,
            StoredSequentialId = level.LevelId,
        };

        this.Write(() => this.Events.Add(e));
        return e;
    }


    /// <summary>
    /// Creates a new LevelPlay event from a <see cref='GameLevel'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelPlayEvent(GameUser userFrom, GameLevel level)
    {
        Event e = new()
        {
            EventType = EventType.LevelPlay,
            StoredDataType = EventDataType.Level,
            Timestamp = this._time.Now,
            User = userFrom,
            StoredSequentialId = level.LevelId,
        };

        this.Write(() => this.Events.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new LevelTag event from a <see cref='GameLevel'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelTagEvent(GameUser userFrom, GameLevel level)
    {
        Event e = new()
        {
            EventType = EventType.LevelTag,
            StoredDataType = EventDataType.Level,
            Timestamp = this._time.Now,
            User = userFrom,
            StoredSequentialId = level.LevelId,
        };

        this.Write(() => this.Events.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new LevelTeamPick event from a <see cref='GameLevel'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelTeamPickEvent(GameUser userFrom, GameLevel level)
    {
        Event e = new()
        {
            EventType = EventType.LevelTeamPick,
            StoredDataType = EventDataType.Level,
            Timestamp = this._time.Now,
            User = userFrom,
            StoredSequentialId = level.LevelId,
        };

        this.Write(() => this.Events.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new LevelRate event from a <see cref='RateLevelRelation'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelRateEvent(GameUser userFrom, RateLevelRelation relation)
    {
        Event e = new()
        {
            EventType = EventType.LevelRate,
            StoredDataType = EventDataType.RateLevelRelation,
            Timestamp = this._time.Now,
            User = userFrom,
            StoredObjectId = relation.RateLevelRelationId,
        };

        this.Write(() => this.Events.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new LevelReview event from a <see cref='GameLevel'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelReviewEvent(GameUser userFrom, GameLevel level)
    {
        Event e = new()
        {
            EventType = EventType.LevelReview,
            StoredDataType = EventDataType.Level,
            Timestamp = this._time.Now,
            User = userFrom,
            StoredSequentialId = level.LevelId,
        };

        this.Write(() => this.Events.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new LevelScore event from a <see cref='GameScore'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelScoreEvent(GameUser userFrom, GameScore score)
    {
        Event e = new()
        {
            EventType = EventType.LevelScore,
            StoredDataType = EventDataType.Score,
            Timestamp = this._time.Now,
            User = userFrom,
            StoredObjectId = score.ScoreId,
        };

        this.Write(() => this.Events.Add(e));
        return e;
    }

    public Event CreatePhotoUploadEvent(GameUser userFrom, GamePhoto photo)
    {
        Event e = new()
        {
            EventType = EventType.PhotoUpload,
            StoredDataType = EventDataType.Photo,
            Timestamp = this._time.Now,
            User = userFrom,
            StoredSequentialId = photo.PhotoId,
        };
        
        this.Write(() => this.Events.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new UserFirstLogin event from a <see cref='GameUser'/>, and adds it to the event list.
    /// </summary>
    public Event CreateUserFirstLoginEvent(GameUser user)
    {
        Event e = new()
        {
            EventType = EventType.UserFirstLogin,
            StoredDataType = EventDataType.User,
            Timestamp = this._time.Now,
            User = user,
            StoredObjectId = user.UserId,
        };

        this.Write(() => this.Events.Add(e));
        return e;
    }
}