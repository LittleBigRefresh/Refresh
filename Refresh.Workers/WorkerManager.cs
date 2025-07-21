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

    private readonly int _workerId;
    
    private Thread? _thread = null;
    private bool _threadShouldRun = false;

    private long _lastContactUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    private readonly List<WorkerJob> _jobs = [];

    public WorkerManager(Logger logger, IDataStore dataStore, GameDatabaseProvider databaseProvider)
    {
        this._dataStore = dataStore;
        this._databaseProvider = databaseProvider;
        this._logger = logger;
        
        using GameDatabaseContext context = this._databaseProvider.GetContext();
        this._workerId = context.CreateWorker();
    }

    public void AddJob<TJob>() where TJob : WorkerJob, new()
    {
        TJob worker = new();
        this._jobs.Add(worker);
    }
    public void AddJob(WorkerJob worker)
    {
        this._jobs.Add(worker);
    }

    private void RunWorkCycle()
    {
        WorkContext context = new()
        {
            Database = this._databaseProvider.GetContext(),
            Logger = this._logger,
            DataStore = this._dataStore,
        };
        
        foreach (WorkerJob job in this._jobs)
        {
            if (!job.CanExecute())
                continue;
            
            this._logger.LogDebug(RefreshContext.Worker, $"Running work cycle for {job.GetType().Name}");
            try
            {
                job.ExecuteJob(context);
                job.FirstCycle = false;
            }
            catch(Exception e)
            {
                this._logger.LogError(RefreshContext.Worker, $"Unhandled exception while running work cycle for {job.GetType().Name}: {e}");
            }
        }
        
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (now - this._lastContactUpdate < 5000) return;

        this._lastContactUpdate = now;
        bool updated = context.Database.MarkWorkerContacted(this._workerId);

        if (!updated)
        {
            this._logger.LogInfo(RefreshContext.Worker, "Worker is shutting down as we've been replaced.");
            this.Stop(false);
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
                    Thread.Sleep(500);
                    this.RunWorkCycle();
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
    
    public void Stop(bool join = true)
    {
        if (this._thread == null) return;
        this._logger.LogDebug(RefreshContext.Worker, "Stopping the worker thread");
        
        this._threadShouldRun = false;
        if(join)
            this._thread.Join();
    }

    public void WaitForExit()
    {
        while(this._thread == null)
            Thread.Sleep(20);

        this._thread.Join();
    }
}