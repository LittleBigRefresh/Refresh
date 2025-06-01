using Refresh.Core.Types.Data;
using TrackingService = Refresh.Interfaces.Workers.RequestTracking.RequestStatisticTrackingService;

namespace Refresh.Interfaces.Workers.RequestTracking;

public class RequestStatisticSubmitWorker : IWorker
{
    public int WorkInterval => 5_000;
    
    public void DoWork(DataContext context)
    {
        (int game, int api) = TrackingService.SubmitAndClearRequests();
        
        context.Database.IncrementGameRequests(game);
        context.Database.IncrementApiRequests(api);
    }
}