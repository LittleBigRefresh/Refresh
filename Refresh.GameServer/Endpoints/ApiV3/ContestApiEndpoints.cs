using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Http;
using MongoDB.Bson;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Types.Contests;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class ContestApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("contests/{id}")]
    [DocSummary("Gets a contest by the contest's unique ID")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.ContestMissingErrorWhen)]
    public ApiResponse<ApiContestResponse> GetContest(RequestContext context, GameDatabaseContext database, string id)
    {
        GameContest? contest = database.GetContestById(id);
        if (contest == null) return ApiNotFoundError.ContestMissingError;
        
        return ApiContestResponse.FromOld(contest);
    }
    
    [ApiV3Endpoint("contests/{id}", HttpMethods.Patch)]
    [DocSummary("Allows an admin/organizer to update their contest details")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.ContestMissingErrorWhen)]
    [DocError(typeof(ApiAuthenticationError), ApiAuthenticationError.NoPermissionsForObjectWhen)]
    public ApiResponse<ApiContestResponse> UpdateContest(RequestContext context, GameDatabaseContext database, string id, ApiContestRequest body, GameUser user)
    {
        GameContest? contest = database.GetContestById(id);
        if (contest == null) return ApiNotFoundError.ContestMissingError;
        
        if (!Equals(user, contest.Organizer) && user.Role != GameUserRole.Admin)
            return ApiAuthenticationError.NoPermissionsForObject;
        
        bool parsed = ObjectId.TryParse(body.OrganizerId, out ObjectId organizerId);
        if (!parsed)
            return ApiValidationError.ObjectIdParseError;
        
        GameUser? organizer = database.GetUserByObjectId(organizerId);
        if (organizer == null)
            return ApiNotFoundError.UserMissingError;
        
        database.UpdateContest(body, contest, organizer);
        
        return ApiContestResponse.FromOld(contest);
    }
}