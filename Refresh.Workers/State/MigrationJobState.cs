namespace Refresh.Workers.State;

public class MigrationJobState
{
    public bool StateInitialized;
    public int Total;
    public int Processed;

    public bool Complete => this.Remaining <= 0;
    public int Remaining => this.Total - this.Processed;
}