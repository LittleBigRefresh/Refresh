using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Data;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Assets;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3.Admin;

public class AdminAssetApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("admin/users/uuid/{uuid}/assets"), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Retrieves a list of assets uploaded by the user.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiListResponse<ApiMinimalGameAssetResponse> GetAssetsByUser(RequestContext context, GameDatabaseContext database, DataContext dataContext,
        [DocSummary("The UUID of the user.")] string uuid)
    {
        GameUser? user = database.GetUserByUuid(uuid);
        if (user == null) return ApiNotFoundError.UserMissingError;
        
        (int skip, int count) = context.GetPageData(1000);

        DatabaseList<GameAsset> assets = database.GetAssetsUploadedByUser(user, skip, count);
        return DatabaseList<ApiMinimalGameAssetResponse>.FromOldList<ApiMinimalGameAssetResponse, GameAsset>(assets, dataContext);
    }
}