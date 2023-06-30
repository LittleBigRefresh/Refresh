using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class UserApiEndpoints : EndpointGroup
{
    /// <summary>
    /// Tries to find a <see cref="GameUser"/> by the username.
    /// </summary>
    /// <param name="username">The username of the user you would like to request.</param>
    /// <returns>An <see cref="ApiGameUserResponse"/>, or if the user was not found, a <see cref="ApiNotFoundError"/>.</returns>
    [ApiV3Endpoint("user/name/{username}")]
    [Authentication(false)]
    public ApiResponse<ApiGameUserResponse> GetUserByName(RequestContext context, GameDatabaseContext database, string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if(user == null) return new ApiNotFoundError();
        
        return ApiGameUserResponse.FromGameUser(user);
    }

    /// <summary>
    /// Tries to find a <see cref="GameUser"/> by the UUID.
    /// </summary>
    /// <param name="uuid">The UUID of the user you would like to request.</param>
    /// <returns>An <see cref="ApiGameUserResponse"/>, or if the user was not found, a <see cref="ApiNotFoundError"/>.</returns>
    [ApiV3Endpoint("user/uuid/{uuid}")]
    [Authentication(false)]
    public ApiResponse<ApiGameUserResponse> GetUserByUuid(RequestContext context, GameDatabaseContext database, string uuid)
    {
        GameUser? user = database.GetUserByUuid(uuid);
        if(user == null) return new ApiNotFoundError();
        
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