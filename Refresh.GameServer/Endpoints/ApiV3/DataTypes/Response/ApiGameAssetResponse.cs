using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameAssetResponse : IApiResponse, IDataConvertableFrom<ApiGameAssetResponse, GameAsset>
{
    public required string AssetHash { get; set; }
    public required ApiGameUserResponse? OriginalUploader { get; set; }
    public required DateTimeOffset UploadDate { get; set; }
    public required GameAssetType AssetType { get; set; }
    
    public static ApiGameAssetResponse? FromOld(GameAsset? old)
    {
        if (old == null) return null;

        return new ApiGameAssetResponse
        {
            AssetHash = old.AssetHash,
            OriginalUploader = ApiGameUserResponse.FromOld(old.OriginalUploader),
            UploadDate = old.UploadDate,
            AssetType = old.AssetType,
        };
    }

    public static IEnumerable<ApiGameAssetResponse> FromOldList(IEnumerable<GameAsset> oldList) => oldList.Select(FromOld)!;
}