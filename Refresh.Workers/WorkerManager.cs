using Bunkum.Core.Storage;
using NotEnoughLogs;
using Refresh.Core;
using Refresh.Database;

namespace Refresh.Workers;

public class WorkerManager
{
    private readonly Logger _logger;
    private readonly IDataStore _dataStore;
    private readonly GameDatabaseProvider _databaseProvider;

    public WorkerManager(Logger logger, IDataStore dataStore, GameDatabaseProvider databaseProvider)
    {
        this._dataStore = dataStore;
        this._databaseProvider = databaseProvider;
        this._logger = logger;
    }

    private Thread? _thread = null;
    private bool _threadShouldRun = false;

    private readonly List<WorkerJob> _workers = [];
    private readonly Dictionary<WorkerJob, long> _lastWorkTimestamps = new();

    public void AddWorker<TWorker>() where TWorker : WorkerJob, new()
    {
        TWorker worker = new();
        this._workers.Add(worker);
    }
    public void AddWorker(WorkerJob worker)
    {
        this._workers.Add(worker);
    }

    private void RunWorkCycle()
    {
        Lazy<WorkContext> workContext = new(() => new WorkContext
        {
            Database = this._databaseProvider.GetContext(),
            Logger = this._logger,
            DataStore = this._dataStore,
        });
        
        foreach (WorkerJob worker in this._workers)
        {
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (this._lastWorkTimestamps.TryGetValue(worker, out long lastWork))
            {
                if(now - lastWork < worker.WorkInterval) continue;
                
                this._lastWorkTimestamps[worker] = now;
            }
            else
            {
                this._lastWorkTimestamps.Add(worker, now);
            }
            
            this._logger.LogTrace(RefreshContext.Worker, "Running work cycle for " + worker.GetType().Name);
            worker.ExecuteJob(workContext.Value);
            worker.FirstCycle = false;
        }
    }

    public void Start()
    {
        this._logger.LogDebug(RefreshContext.Startup, "Starting the worker thread");
        this._threadShouldRun = true;
        Thread thread = new(() =>
        {
            while (this._threadShouldRun)
            {
                try
                {
                    this.RunWorkCycle();
                    Thread.Sleep(100);
                }
                catch(Exception e)
                {
                    this._logger.LogCritical(RefreshContext.Worker, "Critical exception while running work cycle: " + e);
                    this._logger.LogCritical(RefreshContext.Startup, "Waiting for 1 second before trying to run another cycle.");
                    Thread.Sleep(1000);
                }
            }
        });
        
        thread.Start();

        this._thread = thread;
    }
    
    public void Stop()
    {
        if (this._thread == null) return;
        this._logger.LogDebug(RefreshContext.Worker, "Stopping the worker thread");
        
        this._threadShouldRun = false;
        while (this._thread.IsAlive)
        {
            Thread.Sleep(10);
        }
    }
}