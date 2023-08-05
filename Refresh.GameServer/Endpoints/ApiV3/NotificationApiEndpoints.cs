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
    [ApiV3Endpoint("notifications")]
    [DocUsesPageData, DocSummary("Gets a list of notifications stored for the user")]
    public ApiListResponse<ApiGameNotificationResponse> GetNotifications(RequestContext context, GameUser user, GameDatabaseContext database)
    {
        (int skip, int count) = context.GetPageData(true);
        DatabaseList<GameNotification> notifications = database.GetNotificationsByUser(user, count, skip);
        return DatabaseList<ApiGameNotificationResponse>.FromOldList<ApiGameNotificationResponse, GameNotification>(notifications);
    }

    [ApiV3Endpoint("notifications/{uuid}")]
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
    
    [ApiV3Endpoint("notifications/{uuid}", Method.Delete)]
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
    
    [ApiV3Endpoint("notifications", Method.Delete)]
    [DocSummary("Clears all notifications stored for the user")]
    public ApiOkResponse ClearAllNotifications(RequestContext context, GameUser user, GameDatabaseContext database)
    {
        database.DeleteNotificationsByUser(user);
        return new ApiOkResponse();
    }

    [ApiV3Endpoint("admin/announcements", Method.Post), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Creates an announcement that shows up in the Instance API endpoint")]
    public ApiResponse<ApiGameAnnouncementResponse> CreateAnnouncement(RequestContext context, GameDatabaseContext database, ApiGameAnnouncementRequest body)
    {
        GameAnnouncement announcement = database.AddAnnouncement(body.Title, body.Text);
        return ApiGameAnnouncementResponse.FromOld(announcement);
    }

    [ApiV3Endpoint("admin/announcements/{idStr}", Method.Delete), MinimumRole(GameUserRole.Admin)]
    [DocError(typeof(ApiValidationError), ApiValidationError.ObjectIdParseErrorWhen)]
    [DocError(typeof(ApiNotFoundError), "The announcement could not be found")]
    [DocSummary("Removes an announcement")]
    public ApiOkResponse RemoveAnnouncement(RequestContext context, GameDatabaseContext database, string idStr)
    {
        bool parsed = ObjectId.TryParse(idStr, out ObjectId id);
        if (!parsed) return ApiValidationError.ObjectIdParseError;

        GameAnnouncement? announcement = database.GetAnnouncementById(id);
        if (announcement == null) return ApiNotFoundError.Instance;

        database.DeleteAnnouncement(announcement);
        return new ApiOkResponse();
    }
}