using Refresh.GameServer.Types;

namespace Refresh.GameServer.Database;

public partial interface IGameDatabaseContext // Statistics
{
    public RequestStatistics GetRequestStatistics()
    {
        RequestStatistics? statistics = this.All<RequestStatistics>().FirstOrDefault();
        if (statistics != null) return statistics;

        statistics = new RequestStatistics();
        this.Write(() =>
        {
            this.Add(statistics);
        });

        return statistics;
    }
    
    public void IncrementApiRequests()
    {
        RequestStatistics statistics = this.GetRequestStatistics();
        this.Write(() => {
            statistics.TotalRequests++;
            statistics.ApiRequests++;
        });
    }
    public void IncrementLegacyApiRequests()
    {
        RequestStatistics statistics = this.GetRequestStatistics();
        this.Write(() => {
            statistics.TotalRequests++;
            statistics.LegacyApiRequests++;
        });
    }
    public void IncrementGameRequests()
    {
        RequestStatistics statistics = this.GetRequestStatistics();
        this.Write(() => {
            statistics.TotalRequests++;
            statistics.GameRequests++;
        });
    }
}