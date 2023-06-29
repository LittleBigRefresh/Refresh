using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class UserApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("user/name/{username}")]
    [Authentication(false)]
    public ApiResponse<ApiGameUserResponse> GetUserByName(RequestContext context, GameDatabaseContext database, string username) 
        => ApiGameUserResponse.FromGameUser(database.GetUserByUsername(username));

    [ApiV3Endpoint("user/uuid/{uuid}")]
    [Authentication(false)]
    public ApiResponse<ApiGameUserResponse> GetUserByUuid(RequestContext context, GameDatabaseContext database, string uuid) 
        => ApiGameUserResponse.FromGameUser(database.GetUserByUuid(uuid));
    
    [ApiV3Endpoint("user/me")]
    public ApiResponse<ApiGameUserResponse> GetMyUser(RequestContext context, GameUser user)
        => ApiGameUserResponse.FromGameUser(user);
}