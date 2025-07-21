using System.Diagnostics;

namespace Refresh.Workers;

public abstract class StartupJob : WorkerJob
{
    public override int Interval => int.MaxValue;
    public sealed override void ExecuteJob(WorkContext context)
    {
        if (!this.FirstCycle)
            throw new UnreachableException("Invoked startup job twice");

        this.ExecuteStartupJob(context);
    }

    protected abstract void ExecuteStartupJob(WorkContext context);
}