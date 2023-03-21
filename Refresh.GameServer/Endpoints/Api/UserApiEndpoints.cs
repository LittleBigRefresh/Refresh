using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Api;

public class UserApiEndpoints : EndpointGroup
{
    [ApiEndpoint("user/name/{username}")]
    [Authentication(false)]
    public GameUser? GetUserByName(RequestContext context, RealmDatabaseContext database, string username) 
        => database.GetUserByUsername(username);

    [ApiEndpoint("user/uuid/{uuid}")]
    [Authentication(false)]
    public GameUser? GetUserByUuid(RequestContext context, RealmDatabaseContext database, string uuid) 
        => database.GetUserByUuid(uuid);
    
    [ApiEndpoint("user/me")]
    public GameUser GetMyUser(RequestContext context, GameUser user) => user;
}