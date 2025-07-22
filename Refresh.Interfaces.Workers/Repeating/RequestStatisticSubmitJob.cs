using Refresh.Core.Metrics;
using Refresh.Workers;

namespace Refresh.Interfaces.Workers.Repeating;

public class RequestStatisticSubmitJob : RepeatingJob
{
    protected override int Interval => 5_000;
    
    public override void ExecuteJob(WorkContext context)
    {
        (int game, int api) = RequestStatisticTrackingMiddleware.SubmitAndClearRequests();
        
        context.Database.IncrementRequests(api, game);
    }
}