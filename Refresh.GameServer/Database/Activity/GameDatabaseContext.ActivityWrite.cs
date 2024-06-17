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
    public Event CreateLevelUploadEvent(GameUser userFrom, GameLevel gamelevel)
    {
        Event @event = new();
        @event.EventType = EventType.Level_Upload;
        @event.StoredDataType = EventDataType.Level;
        @event.Timestamp = this._time.TimestampMilliseconds;
        @event.User = userFrom;

        @event.StoredSequentialId = gamelevel.LevelId;

        this._realm.Write(() => this._realm.Add(@event));
        return @event;
    }

    /// <summary>
    /// Creates a new Level_Favourite event from a <see cref='GameLevel'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelFavouriteEvent(GameUser userFrom, GameLevel gamelevel)
    {
        Event @event = new();
        @event.EventType = EventType.Level_Favourite;
        @event.StoredDataType = EventDataType.Level;
        @event.Timestamp = this._time.TimestampMilliseconds;
        @event.User = userFrom;

        @event.StoredSequentialId = gamelevel.LevelId;

        this._realm.Write(() => this._realm.Add(@event));
        return @event;
    }

    /// <summary>
    /// Creates a new Level_Unfavourite event from a <see cref='GameLevel'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelUnfavouriteEvent(GameUser userFrom, GameLevel gamelevel)
    {
        Event @event = new();
        @event.EventType = EventType.Level_Unfavourite;
        @event.StoredDataType = EventDataType.Level;
        @event.Timestamp = this._time.TimestampMilliseconds;
        @event.User = userFrom;

        @event.StoredSequentialId = gamelevel.LevelId;

        this._realm.Write(() => this._realm.Add(@event));
        return @event;
    }

    /// <summary>
    /// Creates a new User_Favourite event from a <see cref='GameUser'/>, and adds it to the event list.
    /// </summary>
    public Event CreateUserFavouriteEvent(GameUser userFrom, GameUser gameuser)
    {
        Event @event = new();
        @event.EventType = EventType.User_Favourite;
        @event.StoredDataType = EventDataType.User;
        @event.Timestamp = this._time.TimestampMilliseconds;
        @event.User = userFrom;

        @event.StoredObjectId = gameuser.UserId;

        this._realm.Write(() => this._realm.Add(@event));
        return @event;
    }

    /// <summary>
    /// Creates a new User_Unfavourite event from a <see cref='GameUser'/>, and adds it to the event list.
    /// </summary>
    public Event CreateUserUnfavouriteEvent(GameUser userFrom, GameUser gameuser)
    {
        Event @event = new();
        @event.EventType = EventType.User_Unfavourite;
        @event.StoredDataType = EventDataType.User;
        @event.Timestamp = this._time.TimestampMilliseconds;
        @event.User = userFrom;

        @event.StoredObjectId = gameuser.UserId;

        this._realm.Write(() => this._realm.Add(@event));
        return @event;
    }

    /// <summary>
    /// Creates a new Level_Play event from a <see cref='GameLevel'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelPlayEvent(GameUser userFrom, GameLevel gamelevel)
    {
        Event @event = new();
        @event.EventType = EventType.Level_Play;
        @event.StoredDataType = EventDataType.Level;
        @event.Timestamp = this._time.TimestampMilliseconds;
        @event.User = userFrom;

        @event.StoredSequentialId = gamelevel.LevelId;

        this._realm.Write(() => this._realm.Add(@event));
        return @event;
    }

    /// <summary>
    /// Creates a new Level_Tag event from a <see cref='GameLevel'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelTagEvent(GameUser userFrom, GameLevel gamelevel)
    {
        Event @event = new();
        @event.EventType = EventType.Level_Tag;
        @event.StoredDataType = EventDataType.Level;
        @event.Timestamp = this._time.TimestampMilliseconds;
        @event.User = userFrom;

        @event.StoredSequentialId = gamelevel.LevelId;

        this._realm.Write(() => this._realm.Add(@event));
        return @event;
    }

    /// <summary>
    /// Creates a new Level_TeamPick event from a <see cref='GameLevel'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelTeamPickEvent(GameUser userFrom, GameLevel gamelevel)
    {
        Event @event = new();
        @event.EventType = EventType.Level_TeamPick;
        @event.StoredDataType = EventDataType.Level;
        @event.Timestamp = this._time.TimestampMilliseconds;
        @event.User = userFrom;

        @event.StoredSequentialId = gamelevel.LevelId;

        this._realm.Write(() => this._realm.Add(@event));
        return @event;
    }

    /// <summary>
    /// Creates a new RateLevelRelation_Create event from a <see cref='RateLevelRelation'/>, and adds it to the event list.
    /// </summary>
    public Event CreateRateLevelRelationCreateEvent(GameUser userFrom, RateLevelRelation ratelevelrelation)
    {
        Event @event = new();
        @event.EventType = EventType.RateLevelRelation_Create;
        @event.StoredDataType = EventDataType.RateLevelRelation;
        @event.Timestamp = this._time.TimestampMilliseconds;
        @event.User = userFrom;

        @event.StoredObjectId = ratelevelrelation.RateLevelRelationId;

        this._realm.Write(() => this._realm.Add(@event));
        return @event;
    }

    /// <summary>
    /// Creates a new Level_Review event from a <see cref='GameLevel'/>, and adds it to the event list.
    /// </summary>
    public Event CreateLevelReviewEvent(GameUser userFrom, GameLevel gamelevel)
    {
        Event @event = new();
        @event.EventType = EventType.Level_Review;
        @event.StoredDataType = EventDataType.Level;
        @event.Timestamp = this._time.TimestampMilliseconds;
        @event.User = userFrom;

        @event.StoredSequentialId = gamelevel.LevelId;

        this._realm.Write(() => this._realm.Add(@event));
        return @event;
    }

    /// <summary>
    /// Creates a new SubmittedScore_Create event from a <see cref='GameSubmittedScore'/>, and adds it to the event list.
    /// </summary>
    public Event CreateSubmittedScoreCreateEvent(GameUser userFrom, GameSubmittedScore gamesubmittedscore)
    {
        Event @event = new();
        @event.EventType = EventType.SubmittedScore_Create;
        @event.StoredDataType = EventDataType.Score;
        @event.Timestamp = this._time.TimestampMilliseconds;
        @event.User = userFrom;

        @event.StoredObjectId = gamesubmittedscore.ScoreId;

        this._realm.Write(() => this._realm.Add(@event));
        return @event;
    }

    /// <summary>
    /// Creates a new User_FirstLogin event from a <see cref='GameUser'/>, and adds it to the event list.
    /// </summary>
    public Event CreateUserFirstLoginEvent(GameUser userFrom, GameUser gameuser)
    {
        Event @event = new();
        @event.EventType = EventType.User_FirstLogin;
        @event.StoredDataType = EventDataType.User;
        @event.Timestamp = this._time.TimestampMilliseconds;
        @event.User = userFrom;

        @event.StoredObjectId = gameuser.UserId;

        this._realm.Write(() => this._realm.Add(@event));
        return @event;
    }
}