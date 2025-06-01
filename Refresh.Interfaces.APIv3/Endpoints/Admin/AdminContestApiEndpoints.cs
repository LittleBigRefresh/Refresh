using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Http;
using MongoDB.Bson;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Contests;
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
    [DocError(typeof(ApiValidationError), ApiValidationError.ObjectIdParseErrorWhen)]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiResponse<ApiContestResponse> CreateContest(RequestContext context, GameDatabaseContext database,
        ApiContestRequest body, string id, DataContext dataContext)
    {
        if (database.GetContestById(id) != null)
            return ApiValidationError.ResourceExistsError;
        
        bool parsed = ObjectId.TryParse(body.OrganizerId, out ObjectId organizerId);
        if (!parsed)
            return ApiValidationError.ObjectIdParseError;
        
        GameUser? organizer = database.GetUserByObjectId(organizerId);
        if (organizer == null)
            return ApiNotFoundError.UserMissingError;
        
        GameContest contest = new()
        {
            ContestId = id,
            Organizer = organizer,
            BannerUrl = body.BannerUrl,
            ContestTitle = body.ContestTitle,
            ContestSummary = body.ContestSummary,
            ContestTag = body.ContestTag,
            ContestDetails = body.ContestDetails,
            StartDate = body.StartDate!.Value,
            EndDate = body.EndDate!.Value,
            ContestTheme = body.ContestTheme,
            AllowedGames = body.AllowedGames,
            TemplateLevel = body.TemplateLevelId != null ? database.GetLevelById((int)body.TemplateLevelId) : null,
        };
        
        database.CreateContest(contest);
        
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