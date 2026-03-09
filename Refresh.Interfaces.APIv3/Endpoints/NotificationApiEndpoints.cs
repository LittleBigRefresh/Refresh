using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.RateLimit;
using Bunkum.Protocols.Http;
using MongoDB.Bson;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.RateLimits.Users;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Notifications;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Documentation.Attributes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;
using Refresh.Interfaces.APIv3.Extensions;

namespace Refresh.Interfaces.APIv3.Endpoints;

public class NotificationApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("notifications"), MinimumRole(GameUserRole.Restricted)]
    [DocUsesPageData, DocSummary("Gets a list of notifications stored for the user")]
    [RateLimitSettings(NotificationsEndpointLimits.TimeoutDuration, NotificationsEndpointLimits.ApiRequestAmount, 
                                NotificationsEndpointLimits.BlockDuration, NotificationsEndpointLimits.ApiRequestBucket)]
    public ApiListResponse<ApiGameNotificationResponse> GetNotifications(RequestContext context, GameUser user,
        GameDatabaseContext database, DataContext dataContext)
    {
        (int skip, int count) = context.GetPageData();
        DatabaseList<GameNotification> notifications = database.GetNotificationsByUser(user, count, skip);
        return DatabaseListExtensions.FromOldList<ApiGameNotificationResponse, GameNotification>(notifications, dataContext);
    }

    [ApiV3Endpoint("notifications/{uuid}"), MinimumRole(GameUserRole.Restricted)]
    [DocSummary("Gets a specific notification for a user")]
    [DocError(typeof(ApiValidationError), ApiValidationError.ObjectIdParseErrorWhen)]
    [DocError(typeof(ApiNotFoundError), "The notification cannot be found")]
    [RateLimitSettings(NotificationsEndpointLimits.TimeoutDuration, NotificationsEndpointLimits.ApiRequestAmount, 
                                NotificationsEndpointLimits.BlockDuration, NotificationsEndpointLimits.ApiRequestBucket)]
    public ApiResponse<ApiGameNotificationResponse> GetNotificationByUuid(RequestContext context, GameUser user,
        GameDatabaseContext database,
        [DocSummary("The UUID of the notification")]
        string uuid, DataContext dataContext)
    {
        bool parsed = ObjectId.TryParse(uuid, out ObjectId objectId);
        if (!parsed) return ApiValidationError.ObjectIdParseError;

        GameNotification? notification = database.GetNotificationByUuid(user, objectId);
        if (notification == null) return ApiNotFoundError.Instance;
        if (notification.UserId != user.UserId) return ApiNotFoundError.Instance; // 404 on purpose, notifications should remain private
        
        return ApiGameNotificationResponse.FromOld(notification, dataContext);
    }
    
    [ApiV3Endpoint("notifications/{uuid}", HttpMethods.Delete), MinimumRole(GameUserRole.Restricted)]
    [DocSummary("Clears an individual notification for a user")]
    [DocError(typeof(ApiValidationError), ApiValidationError.ObjectIdParseErrorWhen)]
    [DocError(typeof(ApiNotFoundError), "The notification cannot be found")]
    [RateLimitSettings(NotificationsEndpointLimits.TimeoutDuration, NotificationsEndpointLimits.ApiRequestAmount, 
                                NotificationsEndpointLimits.BlockDuration, NotificationsEndpointLimits.ApiRequestBucket)]
    public ApiOkResponse ClearNotificationByUuid(RequestContext context, GameUser user, GameDatabaseContext database,
        [DocSummary("The UUID of the notification")] string uuid)
    {
        bool parsed = ObjectId.TryParse(uuid, out ObjectId objectId);
        if (!parsed) return ApiValidationError.ObjectIdParseError;
        
        GameNotification? notification = database.GetNotificationByUuid(user, objectId);
        if (notification == null) return ApiNotFoundError.Instance;

        if (notification.UserId != user.UserId) return ApiNotFoundError.Instance;
        
        database.DeleteNotification(notification);
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("notifications", HttpMethods.Delete), MinimumRole(GameUserRole.Restricted)]
    [DocSummary("Clears all notifications stored for the user")]
    [RateLimitSettings(NotificationsEndpointLimits.TimeoutDuration, NotificationsEndpointLimits.ApiRequestAmount, 
                                NotificationsEndpointLimits.BlockDuration, NotificationsEndpointLimits.ApiRequestBucket)]
    public ApiOkResponse ClearAllNotifications(RequestContext context, GameUser user, GameDatabaseContext database)
    {
        database.DeleteNotificationsByUser(user);
        return new ApiOkResponse();
    }
}