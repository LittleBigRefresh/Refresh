using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Http;
using MongoDB.Bson;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Contests;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response;

namespace Refresh.Interfaces.APIv3.Endpoints.Admin;

public class AdminContestApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("admin/contests/{id}", HttpMethods.Post), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Creates a contest.")]
    [DocError(typeof(ApiValidationError), ApiValidationError.ResourceExistsErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.ContestOrganizerIdParseErrorWhen)]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.ContestOrganizerMissingErrorWhen)]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.TemplateLevelMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.ContestDataMissingErrorWhen)]
    public ApiResponse<ApiContestResponse> CreateContest(RequestContext context, GameDatabaseContext database,
        ApiContestCreationRequest body, string id, DataContext dataContext)
    {
        if (database.GetContestById(id) != null)
            return ApiValidationError.ResourceExistsError;
        
        bool organizerParsed = ObjectId.TryParse(body.OrganizerId, out ObjectId organizerId);
        if (!organizerParsed)
            return ApiValidationError.ContestOrganizerIdParseError;
        
        GameUser? organizer = database.GetUserByObjectId(organizerId);
        if (organizer == null)
            return ApiNotFoundError.ContestOrganizerMissingError;

        // Allow template level to either be unspecified or valid, but not an ID which is invalid
        GameLevel? templateLevel = null;
        if (body.TemplateLevelId != null)
        {
            templateLevel = database.GetLevelById(body.TemplateLevelId.Value);
            if (templateLevel == null)
                return ApiNotFoundError.TemplateLevelMissingError;
        }

        GameContest contest = database.CreateContest(id, body, organizer, templateLevel); 
        return ApiContestResponse.FromOld(contest, dataContext);
    }
    
    [ApiV3Endpoint("admin/contests/{id}", HttpMethods.Delete), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Deletes a contest.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.ContestMissingErrorWhen)]
    public ApiOkResponse DeleteContest(RequestContext context, GameDatabaseContext database, string id)
    {
        GameContest? contest = database.GetContestById(id);
        if (contest == null) return ApiNotFoundError.ContestMissingError;
        
        database.DeleteContest(contest);
        
        return new ApiOkResponse();
    }
}