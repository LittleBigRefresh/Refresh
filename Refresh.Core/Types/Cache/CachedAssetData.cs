using Refresh.Database.Models.Assets;

namespace Refresh.Core.Types.Cache;

public class CachedAssetData
{
    public GameAsset? Asset { get; set; } // incase asset doesn't exist
    public DateTimeOffset ExpiresAt { get; set; }
}