namespace Refresh.Database.Models.Relations;

#nullable disable

#if POSTGRES
using PrimaryKeyAttribute = Microsoft.EntityFrameworkCore.PrimaryKeyAttribute;
[PrimaryKey(nameof(Dependent), nameof(Dependency))]
#endif
public partial class AssetDependencyRelation : IRealmObject
{
    public string Dependent { get; set; }
    public string Dependency { get; set; }
}