using Refresh.Database.Models;

namespace Refresh.Database;

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
    
    public void IncrementRequests(int api, int game)
    {
        RequestStatistics statistics = this.GetRequestStatistics();
        this.Write(() => {
            statistics.ApiRequests += api;
            statistics.GameRequests += game;

            statistics.TotalRequests = statistics.ApiRequests + statistics.GameRequests;
        });
    }
}