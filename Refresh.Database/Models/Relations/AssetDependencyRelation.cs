namespace Refresh.GameServer.Types.Assets;

#nullable disable

public partial class AssetDependencyRelation : IRealmObject
{
    public string Dependent { get; set; }
    public string Dependency { get; set; }
}