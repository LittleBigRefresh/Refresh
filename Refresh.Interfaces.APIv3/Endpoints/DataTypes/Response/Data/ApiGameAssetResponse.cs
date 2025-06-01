using Refresh.Core.Types.Data;
using Refresh.Database.Models.Assets;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;

namespace Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Data;

[JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
public class ApiGameAssetResponse : IApiResponse, IDataConvertableFrom<ApiGameAssetResponse, GameAsset>
{
    public required string AssetHash { get; set; }
    public required ApiGameUserResponse? OriginalUploader { get; set; }
    public required DateTimeOffset UploadDate { get; set; }
    public required GameAssetType AssetType { get; set; }
    public required IEnumerable<string> Dependencies { get; set; }
    public required IEnumerable<string> Dependents { get; set; }
    public required ApiAssetFlags AssetFlags { get; set; }
    
    public static ApiGameAssetResponse? FromOld(GameAsset? old, DataContext dataContext)
    {
        if (old == null) return null;

        return new ApiGameAssetResponse
        {
            AssetHash = old.AssetHash,
            OriginalUploader = ApiGameUserResponse.FromOld(old.OriginalUploader, dataContext),
            UploadDate = old.UploadDate,
            AssetType = old.AssetType,
            Dependencies = dataContext.Database.GetAssetDependencies(old),
            Dependents = dataContext.Database.GetAssetDependents(old),
            AssetFlags = new ApiAssetFlags(old.AssetFlags),
        };
    }

    public static IEnumerable<ApiGameAssetResponse> FromOldList(IEnumerable<GameAsset> oldList, DataContext dataContext) => oldList.Select(old => FromOld(old, dataContext)).ToList()!;
}