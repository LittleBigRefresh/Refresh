using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Http;
using MongoDB.Bson;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Notifications;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response;

namespace Refresh.Interfaces.APIv3.Endpoints.Admin;

public class AdminAnnouncementsApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("admin/announcements", HttpMethods.Post), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Creates an announcement that shows up in the Instance API endpoint")]
    public ApiResponse<ApiGameAnnouncementResponse> CreateAnnouncement(RequestContext context,
        GameDatabaseContext database, ApiGameAnnouncementRequest body, DataContext dataContext)
    {
        GameAnnouncement announcement = database.AddAnnouncement(body.Title, body.Text);
        return ApiGameAnnouncementResponse.FromOld(announcement, dataContext);
    }

    [ApiV3Endpoint("admin/announcements/{idStr}", HttpMethods.Delete), MinimumRole(GameUserRole.Admin)]
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