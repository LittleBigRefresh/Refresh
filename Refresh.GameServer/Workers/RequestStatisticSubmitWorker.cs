using Refresh.Core.Types.Data;
using TrackingService = Refresh.GameServer.Services.RequestStatisticTrackingService;

namespace Refresh.GameServer.Workers;

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