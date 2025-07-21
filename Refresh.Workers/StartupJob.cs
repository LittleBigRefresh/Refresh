namespace Refresh.Workers;

public abstract class StartupJob : WorkerJob
{
    public override bool CanExecute() => this.FirstCycle;
}