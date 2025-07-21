namespace Refresh.Workers;

public abstract class WorkerJob
{
    public bool FirstCycle { get; internal set; } = true;

    public virtual bool CanExecute() => true; 

    /// <summary>
    /// Executes the job.
    /// </summary>
    /// <param name="context">Contains data access so workers can interact with the server.</param>
    public abstract void ExecuteJob(WorkContext context);
}