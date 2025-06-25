using Refresh.Common.Helpers;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Assets;

public partial class GameAsset
{
    [Key] public string AssetHash { get; set; } = string.Empty;
    public GameUser? OriginalUploader { get; set; }
    public DateTimeOffset UploadDate { get; set; }
    public bool IsPSP { get; set; }
    public int SizeInBytes { get; set; }
    public GameAssetType AssetType { get; set; }

    public GameAssetFormat AssetFormat { get; set; }

    [NotMapped] 
    public AssetFlags AssetFlags
    {
        get
        {
            AssetFlags flags = AssetSafetyLevelExtensions.FromAssetType(this.AssetType, this.AssetFormat);

            // If the hash is a vanilla game hash, strip the modded flag
            if (VanillaHashChecker.IsVanillaHash(this.AssetHash))
                flags &= ~AssetFlags.Modded;
            
            return flags;
        }
    }

    public string? AsMainlineIconHash { get; set; }
    public string? AsMipIconHash { get; set; }
    
    //NOTE: there's no "as MIP photo hash" because theres no way to browse photos on LBP PSP.
    public string? AsMainlinePhotoHash { get; set; }
}