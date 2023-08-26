using Realms;

namespace Refresh.GameServer.Types;

public partial class SequentialIdStorage : IRealmObject
{
    public string TypeName { get; set; }
    public int SequentialId { get; set; }
}