namespace Refresh.Database.Models;

#nullable disable

[NotMapped]
public partial class SequentialIdStorage
{
    public string TypeName { get; set; }
    public int SequentialId { get; set; }
}