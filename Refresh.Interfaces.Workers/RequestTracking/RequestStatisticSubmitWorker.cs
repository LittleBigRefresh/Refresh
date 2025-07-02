using Refresh.Core.Metrics;
using Refresh.Core.Types.Data;

namespace Refresh.Interfaces.Workers.RequestTracking;

public class RequestStatisticSubmitWorker : IWorker
{
    public int WorkInterval => 5_000;
    
    public void DoWork(DataContext context)
    {
        (int game, int api) = RequestStatisticTrackingMiddleware.SubmitAndClearRequests();
        
        context.Database.IncrementRequests(api, game);
    }
}