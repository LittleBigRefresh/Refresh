namespace Refresh.Database.Models.Relations;

#nullable disable

#if POSTGRES
using PrimaryKeyAttribute = Microsoft.EntityFrameworkCore.PrimaryKeyAttribute;
[PrimaryKey(nameof(Dependent), nameof(Dependency))]
#endif
public partial class AssetDependencyRelation : IRealmObject
{
    #if POSTGRES
    [Required]
    #endif
    public string Dependent { get; set; }
    #if POSTGRES
    [Required]
    #endif
    public string Dependency { get; set; }
}