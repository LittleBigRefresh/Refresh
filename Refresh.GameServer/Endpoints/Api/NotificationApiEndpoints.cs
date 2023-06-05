using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Notifications;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Api;

public class NotificationApiEndpoints : EndpointGroup
{
    [ApiEndpoint("notifications")]
    public IEnumerable<GameNotification> GetNotifications(RequestContext context, GameUser user, GameDatabaseContext database)
    {
        (int skip, int count) = context.GetPageData(true);
        return database.GetNotificationsByUser(user, count, skip);
    }
}