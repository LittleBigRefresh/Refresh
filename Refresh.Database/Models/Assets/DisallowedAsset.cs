using Refresh.Database.Models.Assets;

#nullable disable

public partial class DisallowedAsset
{
    [Key]
    public string AssetHash { get; set; }
    public GameAssetType AssetType { get; set; }

    // Could be a short description of what this asset is (to understand why it's blocked)
    public string Reason { get; set; } // TODO: maybe also add reasons to the other 3 "DisallowedX" entities, separately from ModerationAction?
}