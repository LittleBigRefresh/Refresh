using Refresh.GameServer.Database;
using Refresh.GameServer.Database.Types;
using Refresh.HttpServer;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer.Endpoints;

public class UserEndpoints : EndpointGroup
{
    [GameEndpoint("user/{name}", Method.Get, ContentType.Xml)]
    [RequiresAuthentication]
    public GameUser? GetUser(RequestContext context, RealmDatabaseContext database, string name)
    {
        return database.GetUser(name);
    }
}