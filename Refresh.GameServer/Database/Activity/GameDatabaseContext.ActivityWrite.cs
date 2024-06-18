using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Database;

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
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredSequentialId = level.LevelId,
        };

        this.Write(() => this._realm.Add(e));
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
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredSequentialId = level.LevelId,
        };

        this.Write(() => this._realm.Add(e));
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
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredSequentialId = level.LevelId,
        };

        this.Write(() => this._realm.Add(e));
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
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredObjectId = user.UserId,
        };

        this.Write(() => this._realm.Add(e));
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
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredObjectId = user.UserId,
        };

        this.Write(() => this._realm.Add(e));
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
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredSequentialId = level.LevelId,
        };

        this.Write(() => this._realm.Add(e));
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
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredSequentialId = level.LevelId,
        };

        this.Write(() => this._realm.Add(e));
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
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredSequentialId = level.LevelId,
        };

        this.Write(() => this._realm.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new LevelRate event from a <see cref='RateLevelRelation'/>, and adds it to the event list.
    /// </summary>
    public Event CreateRateLevelEvent(GameUser userFrom, RateLevelRelation relation)
    {
        Event e = new()
        {
            EventType = EventType.LevelRate,
            StoredDataType = EventDataType.RateLevelRelation,
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredObjectId = relation.RateLevelRelationId,
        };

        this.Write(() => this._realm.Add(e));
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
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredSequentialId = level.LevelId,
        };

        this.Write(() => this._realm.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new LevelScore event from a <see cref='GameSubmittedScore'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelScoreEvent(GameUser userFrom, GameSubmittedScore score)
    {
        Event e = new()
        {
            EventType = EventType.LevelScore,
            StoredDataType = EventDataType.Score,
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredObjectId = score.ScoreId,
        };

        this.Write(() => this._realm.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new UserFirstLogin event from a <see cref='GameUser'/>, and adds it to the event list.
    /// </summary>
    public Event CreateUserFirstLoginEvent(GameUser userFrom, GameUser user)
    {
        Event e = new()
        {
            EventType = EventType.UserFirstLogin,
            StoredDataType = EventDataType.User,
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredObjectId = user.UserId,
        };

        this.Write(() => this._realm.Add(e));
        return e;
    }
}