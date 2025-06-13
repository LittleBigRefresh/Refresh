namespace Refresh.Database.Models.Relations;

#nullable disable

#if POSTGRES
using PrimaryKeyAttribute = Microsoft.EntityFrameworkCore.PrimaryKeyAttribute;
[PrimaryKey(nameof(Dependent), nameof(Dependency))]
#endif
public partial class AssetDependencyRelation : IRealmObject
{
    [Required]
    public string Dependent { get; set; }
    [Required]
    public string Dependency { get; set; }
}