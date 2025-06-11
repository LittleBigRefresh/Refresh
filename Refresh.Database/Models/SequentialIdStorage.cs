namespace Refresh.Database.Models;

#nullable disable

public partial class SequentialIdStorage : IRealmObject
{
    [Key] public string TypeName { get; set; }
    public int SequentialId { get; set; }
}