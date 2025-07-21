using Refresh.Core.Metrics;
using Refresh.Workers;

namespace Refresh.Interfaces.Workers;

public class RequestStatisticSubmitJob : WorkerJob
{
    public override int WorkInterval => 5_000;
    
    public override void ExecuteJob(WorkContext context)
    {
        (int game, int api) = RequestStatisticTrackingMiddleware.SubmitAndClearRequests();
        
        context.Database.IncrementRequests(api, game);
    }
}