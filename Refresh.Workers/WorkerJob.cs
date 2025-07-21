namespace Refresh.Workers;

public abstract class WorkerJob
{
    /// <summary>
    /// How often to perform work, in milliseconds
    /// </summary>
    public abstract int WorkInterval { get; }

    public bool FirstCycle { get; internal set; } = true;

    /// <summary>
    /// Executes the job.
    /// </summary>
    /// <param name="context">Contains data access so workers can interact with the server.</param>
    public abstract void ExecuteJob(WorkContext context);
}