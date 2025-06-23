namespace Refresh.Database.Models.Relations;

#nullable disable

[PrimaryKey(nameof(Dependent), nameof(Dependency))]
public partial class AssetDependencyRelation
{
    [Required]
    public string Dependent { get; set; }
    [Required]
    public string Dependency { get; set; }
}