namespace Refresh.Database.Models.Workers;

/// <summary>
/// Information and metadata about a worker.
/// </summary>
public class WorkerInfo
{
    [Key, Required] public int WorkerId { get; set; }
    /// <summary>
    /// The class of worker this is. This determines what types of jobs this worker runs.
    /// </summary>
    public WorkerClass Class { get; set; }
    /// <summary>
    /// When did this worker come online?
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
    /// <summary>
    /// When is the last time we heard from this worker?
    /// </summary>
    public DateTimeOffset LastContact { get; set; }
}