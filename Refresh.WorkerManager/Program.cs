using Bunkum.Core;
using Bunkum.Core.Storage;
using NotEnoughLogs;
using NotEnoughLogs.Behaviour;
using Refresh.Database;
using Refresh.Database.Configuration;
using Refresh.Interfaces.Workers;
using Refresh.Workers;

LoggerConfiguration loggerConfiguration = new()
{
    Behaviour = new QueueLoggingBehaviour(),
#if DEBUG
    MaxLevel = LogLevel.Trace,
#else
    MaxLevel = LogLevel.Info,
#endif
};

using Logger logger = new(loggerConfiguration);

logger.LogInfo(BunkumCategory.Startup, "Starting up worker manager...");

using GameDatabaseProvider database = new(logger, new EmptyDatabaseConfig());
logger.LogInfo(BunkumCategory.Startup, "Initializing database...");
database.Initialize();
logger.LogInfo(BunkumCategory.Startup, "Warming up database...");
database.Warmup();

logger.LogInfo(BunkumCategory.Startup, "Starting worker manager!");
WorkerManager manager = RefreshWorkerManager.Create(logger, new FileSystemDataStore(), database);

manager.Start();
manager.WaitForExit();