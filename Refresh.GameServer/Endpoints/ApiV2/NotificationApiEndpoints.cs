using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using MongoDB.Bson;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Notifications;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV2;

public class NotificationApiEndpoints : EndpointGroup
{
    [ApiV2Endpoint("notifications")]
    public IEnumerable<GameNotification> GetNotifications(RequestContext context, GameUser user, GameDatabaseContext database)
    {
        (int skip, int count) = context.GetPageData(true);
        return database.GetNotificationsByUser(user, count, skip);
    }

    [ApiV2Endpoint("notification/{uuid}")]
    [NullStatusCode(NotFound)]
    public GameNotification? GetNotificationByUuid(RequestContext context, GameUser user, GameDatabaseContext database, string uuid)
    {
        bool parsed = ObjectId.TryParse(uuid, out ObjectId objectId);
        if (!parsed) return null;
        
        return database.GetNotificationByUuid(user, objectId);
    }
    
    [ApiV2Endpoint("notification/{uuid}", Method.Delete)]
    public Response ClearNotificationByUuid(RequestContext context, GameUser user, GameDatabaseContext database, string uuid)
    {
        bool parsed = ObjectId.TryParse(uuid, out ObjectId objectId);
        if (!parsed) return BadRequest;
        
        GameNotification? notification = database.GetNotificationByUuid(user, objectId);
        if (notification == null) return NotFound;
        
        database.DeleteNotification(notification);
        return OK;
    }
    
    [ApiV2Endpoint("notifications", Method.Delete)]
    public Response ClearAllNotifications(RequestContext context, GameUser user, GameDatabaseContext database)
    {
        database.DeleteNotificationsByUser(user);
        return OK;
    }
}