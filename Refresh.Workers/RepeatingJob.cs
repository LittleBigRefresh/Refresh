namespace Refresh.Workers;

public abstract class RepeatingJob : WorkerJob
{
    /// <summary>
    /// How often to perform work, in milliseconds
    /// </summary>
    protected abstract int Interval { get; }

    private long _lastExecute = 0;

    public override bool CanExecute()
    {
        long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if(now - _lastExecute < this.Interval) return false;
                
        this._lastExecute = now;
        return true;
    }
}