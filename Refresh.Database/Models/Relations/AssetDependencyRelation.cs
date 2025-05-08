namespace Refresh.Database.Models.Relations;

#nullable disable

public partial class AssetDependencyRelation : IRealmObject
{
    public string Dependent { get; set; }
    public string Dependency { get; set; }
}