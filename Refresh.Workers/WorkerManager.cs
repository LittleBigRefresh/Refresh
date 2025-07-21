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

    private readonly List<WorkerJob> _jobs = [];
    private readonly Dictionary<WorkerJob, long> _lastJobTimestamps = new();

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
        Lazy<WorkContext> context = new(() => new WorkContext
        {
            Database = this._databaseProvider.GetContext(),
            Logger = this._logger,
            DataStore = this._dataStore,
        });
        
        foreach (WorkerJob job in this._jobs)
        {
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (this._lastJobTimestamps.TryGetValue(job, out long lastJob))
            {
                if(now - lastJob < job.Interval) continue;
                
                this._lastJobTimestamps[job] = now;
            }
            else
            {
                this._lastJobTimestamps.Add(job, now);
            }
            
            this._logger.LogTrace(RefreshContext.Worker, "Running work cycle for " + job.GetType().Name);
            job.ExecuteJob(context.Value);
            job.FirstCycle = false;
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