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
    [Authentication(false)]
    public ActivityPage GetRecentActivity(RequestContext context, RealmDatabaseContext database)
    {
        ActivityPage page = new(database);
        
        foreach (GameUser user in page.Users.Users) user.PrepareForSerialization();
        foreach (GameLevel level in page.Levels.Items) level.PrepareForSerialization();

        return page;
    }
}