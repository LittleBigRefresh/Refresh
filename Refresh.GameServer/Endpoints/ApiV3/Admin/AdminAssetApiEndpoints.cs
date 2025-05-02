using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Refresh.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Data;
using Refresh.GameServer.Types.Assets;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;
using GameDatabaseContext = Refresh.Database.GameDatabaseContext;

namespace Refresh.GameServer.Endpoints.ApiV3.Admin;

public class AdminAssetApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("admin/users/uuid/{uuid}/assets"), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Retrieves a list of assets uploaded by the user.")]
    [DocQueryParam("assetType", "The asset type to filter by. Can be excluded to list all types.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), "The asset type could not be parsed.")]
    public ApiListResponse<ApiMinimalGameAssetResponse> GetAssetsByUser(RequestContext context, GameDatabaseContext database, DataContext dataContext,
        [DocSummary("The UUID of the user.")] string uuid)
    {
        GameUser? user = database.GetUserByUuid(uuid);
        if (user == null) return ApiNotFoundError.UserMissingError;
        
        (int skip, int count) = context.GetPageData(1000);

        DatabaseList<GameAsset> assets;

        string? assetTypeStr = context.QueryString.Get("assetType");
        if (assetTypeStr == null)
        {
            assets = database.GetAssetsUploadedByUser(user, skip, count);
        }
        else
        {
            bool parsed = Enum.TryParse(assetTypeStr, true, out GameAssetType assetType);
            if (!parsed)
                return new ApiValidationError($"The asset type '{assetTypeStr}' couldn't be parsed. Possible values: "
                    + string.Join(", ", Enum.GetNames(typeof(GameAssetType))));
            
            assets = database.GetAssetsUploadedByUser(user, skip, count, assetType);
        }
        
        return DatabaseListExtensions.FromOldList<ApiMinimalGameAssetResponse, GameAsset>(assets, dataContext);
    }
}