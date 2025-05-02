namespace Refresh.Database.Models;

#nullable disable

public partial class SequentialIdStorage : IRealmObject
{
    public string TypeName { get; set; }
    public int SequentialId { get; set; }
}