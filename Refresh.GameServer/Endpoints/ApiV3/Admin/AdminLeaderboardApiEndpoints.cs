using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Endpoints.ApiV3.Admin;

public class AdminLeaderboardApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("admin/scores/{uuid}", HttpMethods.Delete), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Removes a score by the score's UUID.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.ScoreMissingErrorWhen)]
    public ApiOkResponse DeleteScore(RequestContext context, GameDatabaseContext database,
        [DocSummary("The UUID of the score")] string uuid)
    {
        GameSubmittedScore? score = database.GetScoreByUuid(uuid);
        if (score == null) return ApiNotFoundError.Instance;
        
        database.DeleteScore(score);
        
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("admin/users/uuid/{uuid}/scores", HttpMethods.Delete), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Deletes all scores set by a user. Gets user by their UUID.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiOkResponse DeleteScoresSetByUuid(RequestContext context, GameDatabaseContext database,
        [DocSummary("The UUID of the user")] string uuid)
    {
        GameUser? user = database.GetUserByUuid(uuid);
        if (user == null) return ApiNotFoundError.UserMissingError;
        
        database.DeleteScoresSetByUser(user);
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("admin/users/name/{username}/scores", HttpMethods.Delete), MinimumRole(GameUserRole.Admin)]
    [DocSummary("Deletes all scores set by a user. Gets user by their username.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    public ApiOkResponse DeleteScoresSetByUsername(RequestContext context, GameDatabaseContext database,
        [DocSummary("The username of the user")] string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return ApiNotFoundError.UserMissingError;
        
        database.DeleteScoresSetByUser(user);
        return new ApiOkResponse();
    }
}