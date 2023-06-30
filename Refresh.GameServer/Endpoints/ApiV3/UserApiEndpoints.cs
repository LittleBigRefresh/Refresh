using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Documentation;
using Refresh.GameServer.Documentation.Attributes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class UserApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("user/name/{username}"), Authentication(false)]
    [DocSummary("Tries to find a user by the username.")]
    [DocError(typeof(ApiNotFoundError), "The user cannot be found.")]
    public ApiResponse<ApiGameUserResponse> GetUserByName(RequestContext context, GameDatabaseContext database, 
        [DocParam("The username of the user you would like to request.")] string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if(user == null) return ApiNotFoundError.Instance;
        
        return ApiGameUserResponse.FromGameUser(user);
    }
    
    [ApiV3Endpoint("user/uuid/{uuid}"), Authentication(false)]
    [DocSummary("Tries to find a user by the UUID.")]
    [DocError(typeof(ApiNotFoundError), "The user cannot be found.")]
    public ApiResponse<ApiGameUserResponse> GetUserByUuid(RequestContext context, GameDatabaseContext database,
        [DocParam("The UUID of the user you would like to request.")] string uuid)
    {
        GameUser? user = database.GetUserByUuid(uuid);
        if(user == null) return ApiNotFoundError.Instance;
        
        return ApiGameUserResponse.FromGameUser(user);
    }
    
    [ApiV3Endpoint("user/me")]
    [DocSummary("Returns your own user, provided you are authenticated.")]
    public ApiResponse<ApiGameUserResponse> GetMyUser(RequestContext context, GameUser user)
        => ApiGameUserResponse.FromGameUser(user);
}