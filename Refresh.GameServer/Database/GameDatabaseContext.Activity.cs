using JetBrains.Annotations;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Database;

public partial interface IGameDatabaseContext // Activity
{
    [Pure]
    public DatabaseList<Event> GetRecentActivity(int count, int skip, long timestamp, long endTimestamp) => 
        new(this.All<Event>()
            .Where(e => e.Timestamp < timestamp && e.Timestamp >= endTimestamp)
            .OrderByDescending(e => e.Timestamp), skip, count);
    
    [Pure]
    public DatabaseList<Event> GetRecentActivityForLevel(GameLevel level, int count, int skip, long timestamp, long endTimestamp) => 
        new(this.All<Event>()
            .Where(e => e._StoredDataType == 1 && e.StoredSequentialId == level.LevelId)
            .Where(e => e.Timestamp < timestamp && e.Timestamp >= endTimestamp)
            .OrderByDescending(e => e.Timestamp), skip, count);

    public int GetTotalEventCount() => this.All<Event>().Count();
}