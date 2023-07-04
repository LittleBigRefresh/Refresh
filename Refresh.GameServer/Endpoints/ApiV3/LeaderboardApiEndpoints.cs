using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Documentation.Attributes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Errors;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class LeaderboardApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("scores/{id}/{mode}"), Authentication(false)]
    [DocUsesPageData, DocSummary("Gets a list of the top scores on a level.")]
    [DocQueryParam("showAll", "Whether or not to show all scores. If false, only users' best scores will be shown." +
                              "If true, all scores will be shown no matter what.")]
    [DocError(typeof(ApiValidationError), ApiValidationError.LevelMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.ObjectIdParseErrorWhen)]
    public ApiListResponse<ApiGameScoreResponse> GetTopScoresForLevel(RequestContext context, GameDatabaseContext database,
        [DocSummary("The ID of the level")] int id,
        [DocSummary("The leaderboard more (aka the number of players, e.g. 2 for 2-player mode)")] int mode)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiValidationError.LevelMissingError;
        
        (int skip, int count) = context.GetPageData(true);

        bool result = bool.TryParse(context.QueryString.Get("showAll") ?? "false", out bool showAll);
        if (!result) return ApiValidationError.ObjectIdParseError;

        DatabaseList<GameSubmittedScore> scores = database.GetTopScoresForLevel(level, count, skip, (byte)mode, showAll);
        return DatabaseList<ApiGameScoreResponse>.FromOldList<ApiGameScoreResponse, GameSubmittedScore>(scores);
    }

    [ApiV3Endpoint("scores/{uuid}"), Authentication(false)]
    [DocSummary("Gets an individual score by a UUID")]
    [DocError(typeof(ApiNotFoundError), "The score could not be found")]
    public ApiResponse<ApiGameScoreResponse> GetScoreByUuid(RequestContext context, GameDatabaseContext database, 
        [DocSummary("The UUID of the score")] string uuid)
    {
        GameSubmittedScore? score = database.GetScoreByUuid(uuid);
        if (score == null) return ApiNotFoundError.Instance;
        
        return ApiGameScoreResponse.FromOld(score);
    }
}