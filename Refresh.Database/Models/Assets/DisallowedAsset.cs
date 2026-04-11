using Refresh.Database.Models.Assets;

public partial class DisallowedAsset
{
    [Key] public string AssetHash { get; set; } = null!;

    public GameAssetType AssetType { get; set; }

    public string Reason { get; set; } = "";
    public DateTimeOffset DisallowedAt { get; set; }
}