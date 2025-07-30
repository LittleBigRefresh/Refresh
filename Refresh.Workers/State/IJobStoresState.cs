using Refresh.Database.Models.Workers;

namespace Refresh.Workers.State;

public interface IJobStoresState
{
    public string JobId { get; }
    public object JobState { get; set; }
    public Type JobStateType { get; }
    public WorkerClass JobClass { get; }
}