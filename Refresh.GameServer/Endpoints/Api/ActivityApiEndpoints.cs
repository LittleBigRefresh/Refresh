using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Activity;

namespace Refresh.GameServer.Endpoints.Api;

public class ActivityApiEndpoints : EndpointGroup
{
    [ApiEndpoint("activity")]
    [Authentication(false)]
    public IEnumerable<Event> GetRecentActivity(RequestContext context, RealmDatabaseContext database) 
        => database.GetRecentActivity(20, 0);
}