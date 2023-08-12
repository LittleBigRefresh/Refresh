using AttribDoc.Attributes;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using MongoDB.Bson;
using Refresh.GameServer.Database;
using Refresh.GameServer.Documentation.Attributes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Notifications;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class NotificationApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("notifications"), MinimumRole(GameUserRole.Restricted)]
    [DocUsesPageData, DocSummary("Gets a list of notifications stored for the user")]
    public ApiListResponse<ApiGameNotificationResponse> GetNotifications(RequestContext context, GameUser user, GameDatabaseContext database)
    {
        (int skip, int count) = context.GetPageData(true);
        DatabaseList<GameNotification> notifications = database.GetNotificationsByUser(user, count, skip);
        return DatabaseList<ApiGameNotificationResponse>.FromOldList<ApiGameNotificationResponse, GameNotification>(notifications);
    }

    [ApiV3Endpoint("notifications/{uuid}"), MinimumRole(GameUserRole.Restricted)]
    [DocSummary("Gets a specific notification for a user")]
    [DocError(typeof(ApiValidationError), ApiValidationError.ObjectIdParseErrorWhen)]
    [DocError(typeof(ApiNotFoundError), "The notification cannot be found")]
    public ApiResponse<ApiGameNotificationResponse> GetNotificationByUuid(RequestContext context, GameUser user, GameDatabaseContext database,
        [DocSummary("The UUID of the notification")] string uuid)
    {
        bool parsed = ObjectId.TryParse(uuid, out ObjectId objectId);
        if (!parsed) return ApiValidationError.ObjectIdParseError;

        GameNotification? notification = database.GetNotificationByUuid(user, objectId);
        if (notification == null) return ApiNotFoundError.Instance;
        
        return ApiGameNotificationResponse.FromOld(notification);
    }
    
    [ApiV3Endpoint("notifications/{uuid}", Method.Delete), MinimumRole(GameUserRole.Restricted)]
    [DocSummary("Clears an individual notification for a user")]
    [DocError(typeof(ApiValidationError), ApiValidationError.ObjectIdParseErrorWhen)]
    [DocError(typeof(ApiNotFoundError), "The notification cannot be found")]
    public ApiOkResponse ClearNotificationByUuid(RequestContext context, GameUser user, GameDatabaseContext database,
        [DocSummary("The UUID of the notification")] string uuid)
    {
        bool parsed = ObjectId.TryParse(uuid, out ObjectId objectId);
        if (!parsed) return ApiValidationError.ObjectIdParseError;
        
        GameNotification? notification = database.GetNotificationByUuid(user, objectId);
        if (notification == null) return ApiNotFoundError.Instance;
        
        database.DeleteNotification(notification);
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("notifications", Method.Delete), MinimumRole(GameUserRole.Restricted)]
    [DocSummary("Clears all notifications stored for the user")]
    public ApiOkResponse ClearAllNotifications(RequestContext context, GameUser user, GameDatabaseContext database)
    {
        database.DeleteNotificationsByUser(user);
        return new ApiOkResponse();
    }
}