namespace Refresh.Database.Models.Workers;

public class PersistentJobState
{
    [Key, Required] public string JobId { get; set; } = null!;
    [Column(TypeName = "jsonb"), Required] public string State { get; set; } = null!;
}