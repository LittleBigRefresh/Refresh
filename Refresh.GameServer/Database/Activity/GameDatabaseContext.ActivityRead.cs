using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // ActivityRead
{
    public GameUser? GetUserFromEvent(Event @event)
    {
        if (@event.StoredDataType != EventDataType.User)
            throw new InvalidOperationException($"Event does not store the correct data type (expected {nameof(EventDataType.User)})");

        if (@event.StoredObjectId == null)
            throw new InvalidOperationException("Event was not created correctly, expected StoredObjectId to not be null");

        return this._realm.All<GameUser>()
            .FirstOrDefault(l => l.UserId == @event.StoredObjectId);
    }

    public GameLevel? GetLevelFromEvent(Event @event)
    {
        if (@event.StoredDataType != EventDataType.Level)
            throw new InvalidOperationException($"Event does not store the correct data type (expected {nameof(EventDataType.Level)})");

        if (@event.StoredSequentialId == null)
            throw new InvalidOperationException("Event was not created correctly, expected StoredSequentialId to not be null");

        return this._realm.All<GameLevel>()
            .FirstOrDefault(l => l.LevelId == @event.StoredSequentialId.Value);
    }

    public GameSubmittedScore? GetScoreFromEvent(Event @event)
    {
        if (@event.StoredDataType != EventDataType.Score)
            throw new InvalidOperationException($"Event does not store the correct data type (expected {nameof(EventDataType.Score)})");

        if (@event.StoredObjectId == null)
            throw new InvalidOperationException("Event was not created correctly, expected StoredObjectId to not be null");

        return this._realm.All<GameSubmittedScore>()
            .FirstOrDefault(l => l.ScoreId == @event.StoredObjectId);
    }

    public RateLevelRelation? GetRateLevelRelationFromEvent(Event @event)
    {
        if (@event.StoredDataType != EventDataType.RateLevelRelation)
            throw new InvalidOperationException($"Event does not store the correct data type (expected {nameof(EventDataType.RateLevelRelation)})");

        if (@event.StoredObjectId == null)
            throw new InvalidOperationException("Event was not created correctly, expected StoredObjectId to not be null");

        return this._realm.All<RateLevelRelation>()
            .FirstOrDefault(l => l.RateLevelRelationId == @event.StoredObjectId);
    }
}