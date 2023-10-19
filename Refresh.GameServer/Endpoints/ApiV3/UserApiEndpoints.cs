using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Listener.Protocol;
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
    public ApiResponse<ApiGameUserResponse> GetUserByName(RequestContext context, GameDatabaseContext database, 
        [DocSummary("The username of the user")] string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if(user == null) return ApiNotFoundError.UserMissingError;
        
        return ApiGameUserResponse.FromOld(user);
    }
    
    [ApiV3Endpoint("users/uuid/{uuid}"), Authentication(false)]
    [DocSummary("Tries to find a user by the UUID")]
    [DocError(typeof(ApiNotFoundError), "The user cannot be found")]
    public ApiResponse<ApiGameUserResponse> GetUserByUuid(RequestContext context, GameDatabaseContext database,
        [DocSummary("The UUID of the user")] string uuid)
    {
        GameUser? user = database.GetUserByUuid(uuid);
        if(user == null) return ApiNotFoundError.UserMissingError;
        
        return ApiGameUserResponse.FromOld(user);
    }
    
    [ApiV3Endpoint("users/me"), MinimumRole(GameUserRole.Restricted)]
    [DocSummary("Returns your own user, provided you are authenticated")]
    public ApiResponse<ApiExtendedGameUserResponse> GetMyUser(RequestContext context, GameUser user)
        => ApiExtendedGameUserResponse.FromOld(user);
    
    [ApiV3Endpoint("users/me", HttpMethods.Patch)]
    [DocSummary("Updates your profile with the given data")]
    public ApiResponse<ApiExtendedGameUserResponse> UpdateUser(RequestContext context, GameDatabaseContext database, GameUser user, ApiUpdateUserRequest body)
    {
        database.UpdateUserData(user, body);
        return ApiExtendedGameUserResponse.FromOld(user);
    }
}