using System.Net;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Activity;

namespace Refresh.GameServer.Endpoints.Api;

public class ActivityApiEndpoints : EndpointGroup
{
    [ApiEndpoint("activity")]
    [NullStatusCode(HttpStatusCode.BadRequest)]
    [Authentication(false)]
    public ActivityPage? GetRecentActivity(RequestContext context, RealmDatabaseContext database)
    {
        long timestamp = 0;

        string? tsStr = context.QueryString["timestamp"];
        if (tsStr != null && !long.TryParse(tsStr, out timestamp)) return null;

        ActivityPage page = new(database, generateGroups: false, timestamp: timestamp);
        return page;
    }
}