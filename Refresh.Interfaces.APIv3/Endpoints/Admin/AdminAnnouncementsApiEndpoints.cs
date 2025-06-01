using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Http;
using MongoDB.Bson;
using Refresh.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Types.Data;
using Refresh.Database.Models.Notifications;
using Refresh.Database.Models.Users;

namespace Refresh.GameServer.Endpoints.ApiV3.Admin;

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