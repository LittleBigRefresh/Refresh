namespace Refresh.Database.Models.Workers;

public class PersistentJobState
{
    [Key, Required] public string JobId { get; set; } = null!;
    public WorkerClass Class { get; set; } = WorkerClass.Refresh;
    [Column(TypeName = "jsonb"), Required] public string State { get; set; } = null!;
}