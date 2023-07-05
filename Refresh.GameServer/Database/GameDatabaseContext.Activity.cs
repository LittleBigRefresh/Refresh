using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Activity
{
    public DatabaseList<Event> GetRecentActivity(int count, int skip, long timestamp, long endTimestamp) => 
        new(this._realm.All<Event>()
            .Where(e => e.Timestamp < timestamp && e.Timestamp >= endTimestamp)
            .OrderBy(e => e.Timestamp), skip, count);
    
    public DatabaseList<Event> GetRecentActivityForLevel(GameLevel level, int count, int skip, long timestamp, long endTimestamp) => 
        new(this._realm.All<Event>()
            .Where(e => e.StoredDataType == EventDataType.Level && e.StoredSequentialId == level.LevelId)
            .Where(e => e.Timestamp < timestamp && e.Timestamp >= endTimestamp)
            .OrderBy(e => e.Timestamp), skip, count);
}