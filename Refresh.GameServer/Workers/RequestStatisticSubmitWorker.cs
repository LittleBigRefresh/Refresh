using Bunkum.Core.Storage;
using NotEnoughLogs;
using Refresh.GameServer.Database;
using TrackingService = Refresh.GameServer.Services.RequestStatisticTrackingService;

namespace Refresh.GameServer.Workers;

public class RequestStatisticSubmitWorker : IWorker
{
    public int WorkInterval => 5_000;
    
    public void DoWork(Logger logger, IDataStore dataStore, GameDatabaseContext database)
    {
        (int game, int api) = TrackingService.SubmitAndClearRequests();
        
        database.IncrementGameRequests(game);
        database.IncrementApiRequests(api);
    }
}