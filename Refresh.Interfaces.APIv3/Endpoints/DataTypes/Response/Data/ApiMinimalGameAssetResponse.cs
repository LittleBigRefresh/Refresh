using Refresh.Database.Models.Assets;
using Refresh.GameServer.Types.Data;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Data;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiMinimalGameAssetResponse : IApiResponse, IDataConvertableFrom<ApiMinimalGameAssetResponse, GameAsset>
{
    public required string AssetHash { get; set; }
    public required string? OriginalUploaderId { get; set; }
    public required DateTimeOffset UploadDate { get; set; }
    public required GameAssetType AssetType { get; set; }
    
    public static ApiMinimalGameAssetResponse? FromOld(GameAsset? old, DataContext dataContext)
    {
        if (old == null)
            return null;

        return new ApiMinimalGameAssetResponse
        {
            AssetHash = old.AssetHash,
            OriginalUploaderId = old.OriginalUploader?.UserId.ToString(),
            UploadDate = old.UploadDate,
            AssetType = old.AssetType,
        };
    }

    public static IEnumerable<ApiMinimalGameAssetResponse> FromOldList(IEnumerable<GameAsset> oldList, DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}