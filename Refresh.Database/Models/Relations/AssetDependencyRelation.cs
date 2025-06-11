namespace Refresh.Database.Models.Relations;

#if POSTGRES
using PrimaryKeyAttribute = Microsoft.EntityFrameworkCore.PrimaryKeyAttribute;
#endif

#nullable disable

[PrimaryKey(nameof(Dependent), nameof(Dependency))]
public partial class AssetDependencyRelation : IRealmObject
{
    public string Dependent { get; set; }
    public string Dependency { get; set; }
}