using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV2;

public class UserApiEndpoints : EndpointGroup
{
    [ApiV2Endpoint("user/name/{username}")]
    [Authentication(false)]
    public GameUser? GetUserByName(RequestContext context, GameDatabaseContext database, string username) 
        => database.GetUserByUsername(username);

    [ApiV2Endpoint("user/uuid/{uuid}")]
    [Authentication(false)]
    public GameUser? GetUserByUuid(RequestContext context, GameDatabaseContext database, string uuid) 
        => database.GetUserByUuid(uuid);
    
    [ApiV2Endpoint("user/me")]
    public GameUser GetMyUser(RequestContext context, GameUser user) => user;
}