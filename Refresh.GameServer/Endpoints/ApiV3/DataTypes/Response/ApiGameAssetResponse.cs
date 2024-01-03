using Bunkum.Core.Storage;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameAssetResponse : IApiResponse, IDataConvertableFrom<ApiGameAssetResponse, GameAsset>
{
    public required string AssetHash { get; set; }
    public required ApiGameUserResponse? OriginalUploader { get; set; }
    public required DateTimeOffset UploadDate { get; set; }
    public required GameAssetType AssetType { get; set; }
    public required IEnumerable<string> Dependencies { get; set; }
    
    public static ApiGameAssetResponse? FromOld(GameAsset? old)
    {
        if (old == null) return null;

        return new ApiGameAssetResponse
        {
            AssetHash = old.AssetHash,
            OriginalUploader = ApiGameUserResponse.FromOld(old.OriginalUploader),
            UploadDate = old.UploadDate,
            AssetType = old.AssetType,
            Dependencies = old.Dependencies,
        };
    }

    public static ApiGameAssetResponse? FromOldWithExtraData(GameAsset? old, GameDatabaseContext database, IDataStore dataStore)
    {
        if (old == null) return null;

        ApiGameAssetResponse response = FromOld(old)!;
        response.FillInExtraData(database, dataStore);

        return response;
    }

    public void FillInExtraData(GameDatabaseContext database, IDataStore dataStore)
    {
        this.OriginalUploader?.FillInExtraData(database, dataStore);
    }

    public static IEnumerable<ApiGameAssetResponse> FromOldList(IEnumerable<GameAsset> oldList) => oldList.Select(FromOld)!;
}