using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Data;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiAssetFlags
{
    public ApiAssetFlags(AssetFlags flags)
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
}