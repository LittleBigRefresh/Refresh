using Refresh.GameServer.Types.Activity;

namespace Refresh.GameServer.Database;

public partial class RealmDatabaseContext // Activity
{
    public IEnumerable<Event> GetRecentActivity(int count, int skip) => 
        this._realm.All<Event>()
            .AsEnumerable()
            .Skip(skip)
            .Take(count);
}