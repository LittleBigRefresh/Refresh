namespace Refresh.GameServer.Types;

#nullable disable

public partial class SequentialIdStorage : IRealmObject
{
    public string TypeName { get; set; }
    public int SequentialId { get; set; }
}