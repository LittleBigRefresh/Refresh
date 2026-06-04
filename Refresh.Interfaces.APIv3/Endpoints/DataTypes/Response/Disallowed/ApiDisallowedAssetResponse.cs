using Refresh.Core.Types.Data;
using Refresh.Database.Models.Assets;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Disallowed;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiDisallowedAssetResponse : IApiResponse, IDataConvertableFrom<ApiDisallowedAssetResponse, DisallowedAsset>
{
    public required string AssetHash { get; set; }
    public required string AssetType { get; set; }
    public required string Reason { get; set; }
    public required DateTimeOffset DisallowedAt { get; set; }
    
    public static ApiDisallowedAssetResponse? FromOld(DisallowedAsset? old, DataContext dataContext)
    {
        if (old == null) return null;

        return new ApiDisallowedAssetResponse
        {
            AssetHash = old.AssetHash,
            AssetType = old.AssetType.ToString(),
            Reason = old.Reason,
            DisallowedAt = old.DisallowedAt,
        };
    }

    public static IEnumerable<ApiDisallowedAssetResponse> FromOldList(IEnumerable<DisallowedAsset> oldList, DataContext dataContext)
        => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}