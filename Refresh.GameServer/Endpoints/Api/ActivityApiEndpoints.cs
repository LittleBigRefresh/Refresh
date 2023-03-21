using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Activity;

namespace Refresh.GameServer.Endpoints.Api;

public class ActivityApiEndpoints : EndpointGroup
{
    [ApiEndpoint("activity")]
    [Authentication(false)]
    public ActivityPage GetRecentActivity(RequestContext context, RealmDatabaseContext database)
    {
        ActivityPage page = new(database, generateGroups: false);
        return page;
    }
}