using Refresh.Database.Models.Assets;

namespace Refresh.GameServer.Configuration;

public class ConfigAssetFlags
{
    public ConfigAssetFlags() {}
    
    public ConfigAssetFlags(AssetFlags flags)
    {
        this.Dangerous = (flags & AssetFlags.Dangerous) != 0;
        this.Media = (flags & AssetFlags.Media) != 0;
        this.Modded = (flags & AssetFlags.Modded) != 0;
    }
    
    /// <summary>
    /// This asset can be dangerous to end users.
    /// </summary>
    public bool Dangerous { get; set; }
    
    /// <summary>
    /// This asset is a media-type asset, e.g. a PNG or TEX.
    /// </summary>
    public bool Media { get; set; }
    
    /// <summary>
    /// This asset will only ever be created by mods.
    /// </summary>
    public bool Modded { get; set; }
    
    public AssetFlags ToAssetFlags()
    {
        return (this.Dangerous ? AssetFlags.Dangerous : AssetFlags.None) |
               (this.Modded ? AssetFlags.Modded : AssetFlags.None) |
               (this.Media ? AssetFlags.Media : AssetFlags.None);
    }
}