using System.Net;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game;

public class ActivityEndpoints : EndpointGroup
{
    [GameEndpoint("stream", ContentType.Xml)]
    [NullStatusCode(HttpStatusCode.BadRequest)]
    [Authentication(false)]
    public ActivityPage? GetRecentActivity(RequestContext context, RealmDatabaseContext database)
    {
        long timestamp = 0;

        string? tsStr = context.QueryString["timestamp"];
        if (tsStr != null && !long.TryParse(tsStr, out timestamp)) return null;
        
        ActivityPage page = new(database, timestamp: timestamp);
        
        foreach (GameUser user in page.Users.Users) user.PrepareForSerialization();
        foreach (GameLevel level in page.Levels.Items) level.PrepareForSerialization();

        return page;
    }
}