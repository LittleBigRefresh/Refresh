using Realms;

namespace Refresh.GameServer.Types.Assets;

public partial class AssetDependencyRelation : IRealmObject
{
    public string Dependent { get; set; }
    public string Dependency { get; set; }
}