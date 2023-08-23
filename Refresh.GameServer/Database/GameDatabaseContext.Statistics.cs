using Refresh.GameServer.Types;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Statistics
{
    public RequestStatistics GetRequestStatistics()
    {
        RequestStatistics? statistics = this._realm.All<RequestStatistics>().FirstOrDefault();
        if (statistics != null) return statistics;

        statistics = new RequestStatistics();
        this._realm.Write(() =>
        {
            this._realm.Add(statistics);
        });

        return statistics;
    }
    
    public void IncrementApiRequests()
    {
        RequestStatistics statistics = this.GetRequestStatistics();
        this._realm.Write(() => {
            statistics.TotalRequests++;
            statistics.ApiRequests++;
        });
    }
    public void IncrementLegacyApiRequests()
    {
        RequestStatistics statistics = this.GetRequestStatistics();
        this._realm.Write(() => {
            statistics.TotalRequests++;
            statistics.LegacyApiRequests++;
        });
    }
    public void IncrementGameRequests()
    {
        RequestStatistics statistics = this.GetRequestStatistics();
        this._realm.Write(() => {
            statistics.TotalRequests++;
            statistics.GameRequests++;
        });
    }
}