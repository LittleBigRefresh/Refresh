using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Documentation;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class UserApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("user/name/{username}"), Authentication(false)]
    [ApiDocSummary("Tries to find a user by the username.")]
    [ApiDocError(typeof(ApiNotFoundError), "The user cannot be found.")]
    public ApiResponse<ApiGameUserResponse> GetUserByName(RequestContext context, GameDatabaseContext database, 
        [ApiDocParam("The username of the user you would like to request.")] string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if(user == null) return ApiNotFoundError.Instance;
        
        return ApiGameUserResponse.FromGameUser(user);
    }
    
    [ApiV3Endpoint("user/uuid/{uuid}"), Authentication(false)]
    [ApiDocSummary("Tries to find a user by the UUID.")]
    [ApiDocError(typeof(ApiNotFoundError), "The user cannot be found.")]
    public ApiResponse<ApiGameUserResponse> GetUserByUuid(RequestContext context, GameDatabaseContext database,
        [ApiDocParam("The UUID of the user you would like to request.")] string uuid)
    {
        GameUser? user = database.GetUserByUuid(uuid);
        if(user == null) return ApiNotFoundError.Instance;
        
        return ApiGameUserResponse.FromGameUser(user);
    }

    /// <summary>
    /// Returns your own <see cref="GameUser"/>, provided you are authenticated.
    /// </summary>
    /// <returns>An <see cref="ApiGameUserResponse"/> containing your user.</returns>
    [ApiV3Endpoint("user/me")]
    public ApiResponse<ApiGameUserResponse> GetMyUser(RequestContext context, GameUser user)
        => ApiGameUserResponse.FromGameUser(user);
}