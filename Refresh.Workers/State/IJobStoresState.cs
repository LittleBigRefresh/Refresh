namespace Refresh.Workers.State;

public interface IJobStoresState
{
    public string JobId { get; set; }
    public object JobState { get; protected internal set; }
    public Type JobStateType { get; }
}