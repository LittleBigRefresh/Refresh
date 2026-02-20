using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Http;
using Refresh.Core.Authentication.Permission;
using Refresh.Database;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Documentation.Descriptions;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;

namespace Refresh.Interfaces.APIv3.Endpoints.Admin;

public class AdminLeaderboardApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("admin/scores/{uuid}", HttpMethods.Delete), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Removes a score by the score's UUID.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.ScoreMissingErrorWhen)]
    public ApiOkResponse DeleteScore(RequestContext context, GameDatabaseContext database,
        [DocSummary("The UUID of the score")] string uuid)
    {
        GameScore? score = database.GetScoreByUuid(uuid);
        if (score == null) return ApiNotFoundError.Instance;
        
        database.DeleteScore(score);
        
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("admin/users/{idType}/{id}/scores", HttpMethods.Delete), MinimumRole(GameUserRole.Moderator)]
    [DocSummary("Deletes all scores set by a user, specified by UUID or username.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiOkResponse DeleteScoresSetByUser(RequestContext context, GameDatabaseContext database,
        [DocSummary(SharedParamDescriptions.UserIdParam)] string id, 
        [DocSummary(SharedParamDescriptions.UserIdTypeParam)] string idType)
    {
        GameUser? user = database.GetUserByIdAndType(idType, id);
        if (user == null) return ApiNotFoundError.UserMissingError;
        
        database.DeleteScoresSetByUser(user);
        return new ApiOkResponse();
    }
}