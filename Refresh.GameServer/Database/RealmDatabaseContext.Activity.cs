using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Database;

public partial class RealmDatabaseContext // Activity
{
    public GameLevel? GetEventGameLevel(Event @event)
    {
        if (@event.StoredDataType != EventDataType.Level)
            throw new InvalidOperationException("Event does not store the correct data type");

        if (@event.StoredSequentialId == null)
            throw new InvalidOperationException("Event was not created correctly");

        return this._realm.All<GameLevel>()
            .FirstOrDefault(l => l.LevelId == @event.StoredSequentialId.Value);
    }
}