using Bunkum.Core.Storage;
using NotEnoughLogs;
using Refresh.Database;
using Refresh.Interfaces.Workers.Migrations;
using Refresh.Interfaces.Workers.Repeating;
using Refresh.Workers;

namespace Refresh.Interfaces.Workers;

public static class RefreshWorkerManager
{
    public static WorkerManager Create(Logger logger, IDataStore dataStore, GameDatabaseProvider databaseProvider)
    {
        WorkerManager manager = new(logger, dataStore, databaseProvider);
        
        manager.AddJob<PunishmentExpiryJob>();
        manager.AddJob<CleanupExpiredObjectsJob>();
        manager.AddJob<CoolLevelsJob>();
        manager.AddJob<RequestStatisticSubmitJob>();
        manager.AddJob<ObjectStatisticsJob>();
        
        manager.AddJob<BackfillRevisionMigration>();
        manager.AddJob<EnsureDeletedUsersDeletedMigration>();
        
        return manager;
    }
}