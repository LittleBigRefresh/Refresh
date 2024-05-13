using Bunkum.Core.Storage;
using NotEnoughLogs;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;

using TrackingService = Refresh.GameServer.Services.RequestStatisticTrackingService;

namespace Refresh.GameServer.Workers;

public class RequestStatisticSubmitWorker : IWorker
{
    public int WorkInterval => 5_000;
    
    public void DoWork(Logger logger, IDataStore dataStore, GameDatabaseContext database)
    {
        lock (TrackingService.TrackerLock)
        {
            database.IncrementGameRequests(TrackingService.GameRequestsToSubmit);
            database.IncrementApiRequests(TrackingService.ApiRequestsToSubmit);
            
            TrackingService.ClearRequests();
        }
    }
}