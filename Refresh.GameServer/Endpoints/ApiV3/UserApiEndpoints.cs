using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Storage;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class UserApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("users/name/{username}"), Authentication(false)]
    [DocSummary("Tries to find a user by the username")]
    [DocError(typeof(ApiNotFoundError), "The user cannot be found")]
    public ApiResponse<ApiGameUserResponse> GetUserByName(RequestContext context, IGameDatabaseContext database, IDataStore dataStore,
        [DocSummary("The username of the user")] string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if(user == null) return ApiNotFoundError.UserMissingError;
        
        return ApiGameUserResponse.FromOldWithExtraData(user, database, dataStore);
    }
    
    [ApiV3Endpoint("users/uuid/{uuid}"), Authentication(false)]
    [DocSummary("Tries to find a user by the UUID")]
    [DocError(typeof(ApiNotFoundError), "The user cannot be found")]
    public ApiResponse<ApiGameUserResponse> GetUserByUuid(RequestContext context, IGameDatabaseContext database, IDataStore dataStore,
        [DocSummary("The UUID of the user")] string uuid)
    {
        GameUser? user = database.GetUserByUuid(uuid);
        if(user == null) return ApiNotFoundError.UserMissingError;
        
        return ApiGameUserResponse.FromOldWithExtraData(user, database, dataStore);
    }
    
    [ApiV3Endpoint("users/me"), MinimumRole(GameUserRole.Restricted)]
    [DocSummary("Returns your own user, provided you are authenticated")]
    public ApiResponse<ApiExtendedGameUserResponse> GetMyUser(RequestContext context, GameUser user, IGameDatabaseContext database, IDataStore dataStore)
        => ApiExtendedGameUserResponse.FromOldWithExtraData(user, database, dataStore);
    
    [ApiV3Endpoint("users/me", HttpMethods.Patch)]
    [DocSummary("Updates your profile with the given data")]
    public ApiResponse<ApiExtendedGameUserResponse> UpdateUser(RequestContext context, IGameDatabaseContext database, GameUser user, ApiUpdateUserRequest body, IDataStore dataStore)
    {
        if (body.IconHash != null && database.GetAssetFromHash(body.IconHash) == null)
            return ApiNotFoundError.Instance;

        database.UpdateUserData(user, body);
        return ApiExtendedGameUserResponse.FromOldWithExtraData(user, database, dataStore);
    }
}