using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Database;

public partial class RealmDatabaseContext // Activity
{
    public IEnumerable<Event> GetRecentActivity(int count, int skip, long timestamp, long endTimestamp) => 
        this._realm.All<Event>()
            .Where(e => e.Timestamp < timestamp && e.Timestamp >= endTimestamp)
            .OrderBy(e => e.Timestamp)
            .AsEnumerable()
            .Skip(skip)
            .Take(count);
    
    public IEnumerable<Event> GetRecentActivityForLevel(GameLevel level, int count, int skip, long timestamp, long endTimestamp) => 
        this._realm.All<Event>()
            .Where(e => e.StoredDataType == EventDataType.Level && e.StoredSequentialId == level.LevelId)
            .Where(e => e.Timestamp < timestamp && e.Timestamp >= endTimestamp)
            .OrderBy(e => e.Timestamp)
            .AsEnumerable()
            .Skip(skip)
            .Take(count);
}