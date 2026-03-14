using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Documentation.Descriptions;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Data;
using Refresh.Interfaces.APIv3.Extensions;

namespace Refresh.Interfaces.APIv3.Endpoints.Admin;

public class AdminAssetApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("admin/users/{idType}/{id}/assets"), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Retrieves a list of assets uploaded by the user.")]
    [DocQueryParam("assetType", "The asset type to filter by. Can be excluded to list all types.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), "The asset type could not be parsed.")]
    public ApiListResponse<ApiMinimalGameAssetResponse> GetAssetsByUser(RequestContext context, GameDatabaseContext database, DataContext dataContext,
        [DocSummary(SharedParamDescriptions.UserIdParam)] string id, 
        [DocSummary(SharedParamDescriptions.UserIdTypeParam)] string idType)
    {
        GameUser? user = database.GetUserByIdAndType(idType, id);
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