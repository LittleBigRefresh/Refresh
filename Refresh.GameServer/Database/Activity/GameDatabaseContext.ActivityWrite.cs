using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // ActivityWrite
{
    /// <summary>
    /// Creates a new Level_Upload event from a <see cref='GameLevel'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelUploadEvent(GameUser userFrom, GameLevel level)
    {
        Event e = new()
        {
            EventType = EventType.Level_Upload,
            StoredDataType = EventDataType.Level,
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredSequentialId = level.LevelId,
        };

        this._realm.Write(() => this._realm.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new Level_Favourite event from a <see cref='GameLevel'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelFavouriteEvent(GameUser userFrom, GameLevel level)
    {
        Event e = new()
        {
            EventType = EventType.Level_Favourite,
            StoredDataType = EventDataType.Level,
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredSequentialId = level.LevelId,
        };

        this._realm.Write(() => this._realm.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new Level_Unfavourite event from a <see cref='GameLevel'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelUnfavouriteEvent(GameUser userFrom, GameLevel level)
    {
        Event e = new()
        {
            EventType = EventType.Level_Unfavourite,
            StoredDataType = EventDataType.Level,
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredSequentialId = level.LevelId,
        };

        this._realm.Write(() => this._realm.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new User_Favourite event from a <see cref='GameUser'/>, and adds it to the event list.
    /// </summary>
    public Event CreateUserFavouriteEvent(GameUser userFrom, GameUser user)
    {
        Event e = new()
        {
            EventType = EventType.User_Favourite,
            StoredDataType = EventDataType.User,
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredObjectId = user.UserId,
        };

        this._realm.Write(() => this._realm.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new User_Unfavourite event from a <see cref='GameUser'/>, and adds it to the event list.
    /// </summary>
    public Event CreateUserUnfavouriteEvent(GameUser userFrom, GameUser user)
    {
        Event e = new()
        {
            EventType = EventType.User_Unfavourite,
            StoredDataType = EventDataType.User,
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredObjectId = user.UserId,
        };

        this._realm.Write(() => this._realm.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new Level_Play event from a <see cref='GameLevel'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelPlayEvent(GameUser userFrom, GameLevel level)
    {
        Event e = new()
        {
            EventType = EventType.Level_Play,
            StoredDataType = EventDataType.Level,
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredSequentialId = level.LevelId,
        };

        this._realm.Write(() => this._realm.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new Level_Tag event from a <see cref='GameLevel'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelTagEvent(GameUser userFrom, GameLevel level)
    {
        Event e = new()
        {
            EventType = EventType.Level_Tag,
            StoredDataType = EventDataType.Level,
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredSequentialId = level.LevelId,
        };

        this._realm.Write(() => this._realm.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new Level_TeamPick event from a <see cref='GameLevel'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelTeamPickEvent(GameUser userFrom, GameLevel level)
    {
        Event e = new()
        {
            EventType = EventType.Level_TeamPick,
            StoredDataType = EventDataType.Level,
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredSequentialId = level.LevelId,
        };

        this._realm.Write(() => this._realm.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new Level_Rate event from a <see cref='RateLevelRelation'/>, and adds it to the event list.
    /// </summary>
    public Event CreateRateLevelEvent(GameUser userFrom, RateLevelRelation relation)
    {
        Event e = new()
        {
            EventType = EventType.Level_Rate,
            StoredDataType = EventDataType.RateLevelRelation,
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredObjectId = relation.RateLevelRelationId,
        };

        this._realm.Write(() => this._realm.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new Level_Review event from a <see cref='GameLevel'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelReviewEvent(GameUser userFrom, GameLevel level)
    {
        Event e = new()
        {
            EventType = EventType.Level_Review,
            StoredDataType = EventDataType.Level,
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredSequentialId = level.LevelId,
        };

        this._realm.Write(() => this._realm.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new Level_Score event from a <see cref='GameSubmittedScore'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelScoreEvent(GameUser userFrom, GameSubmittedScore score)
    {
        Event e = new()
        {
            EventType = EventType.Level_Score,
            StoredDataType = EventDataType.Score,
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredObjectId = score.ScoreId,
        };

        this._realm.Write(() => this._realm.Add(e));
        return e;
    }

    /// <summary>
    /// Creates a new User_FirstLogin event from a <see cref='GameUser'/>, and adds it to the event list.
    /// </summary>
    public Event CreateUserFirstLoginEvent(GameUser userFrom, GameUser user)
    {
        Event e = new()
        {
            EventType = EventType.User_FirstLogin,
            StoredDataType = EventDataType.User,
            Timestamp = this._time.TimestampMilliseconds,
            User = userFrom,
            StoredObjectId = user.UserId,
        };

        this._realm.Write(() => this._realm.Add(e));
        return e;
    }
}