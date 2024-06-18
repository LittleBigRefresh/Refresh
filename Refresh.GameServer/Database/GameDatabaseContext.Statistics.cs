using Refresh.GameServer.Types;

namespace Refresh.GameServer.Database;

public partial class GameDatabaseContext // Statistics
{
    public RequestStatistics GetRequestStatistics()
    {
        RequestStatistics? statistics = this.RequestStatistics.FirstOrDefault();
        if (statistics != null) return statistics;

        statistics = new RequestStatistics();
        this.Write(() =>
        {
            this.RequestStatistics.Add(statistics);
        });

        return statistics;
    }
    
    public void IncrementApiRequests(int count)
    {
        RequestStatistics statistics = this.GetRequestStatistics();
        this.Write(() => {
            statistics.TotalRequests += count;
            statistics.ApiRequests += count;
        });
    }
    
    public void IncrementGameRequests(int count)
    {
        RequestStatistics statistics = this.GetRequestStatistics();
        this.Write(() => {
            statistics.TotalRequests += count;
            statistics.GameRequests += count;
        });
    }
}