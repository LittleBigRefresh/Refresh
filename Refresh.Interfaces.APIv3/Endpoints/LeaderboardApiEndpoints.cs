using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Storage;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Interfaces.APIv3.Documentation.Attributes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Levels;
using Refresh.Interfaces.APIv3.Extensions;

namespace Refresh.Interfaces.APIv3.Endpoints;

public class LeaderboardApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("scores/{id}/{mode}"), Authentication(false)]
    [DocUsesPageData, DocSummary("Gets a list of the top scores on a level.")]
    [DocQueryParam("showAll", "Whether or not to show all scores. If false, only users' best scores will be shown." +
                              "If true, all scores will be shown no matter what. False by default.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), "The boolean 'showAll' could not be parsed by the server.")]
    public ApiListResponse<ApiGameScoreResponse> GetTopScoresForLevel(RequestContext context,
        GameDatabaseContext database, IDataStore dataStore,
        [DocSummary("The ID of the level")] int id,
        [DocSummary("The leaderboard more (aka the number of players, e.g. 2 for 2-player mode)")]
        int mode, DataContext dataContext)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;
        
        (int skip, int count) = context.GetPageData();

        bool result = bool.TryParse(context.QueryString.Get("showAll") ?? "false", out bool showAll);
        if (!result) return ApiValidationError.BooleanParseError;

        DatabaseList<GameScore> scores = database.GetTopScoresForLevel(level, count, skip, (byte)mode, showAll);
        DatabaseList<ApiGameScoreResponse> ret = DatabaseListExtensions.FromOldList<ApiGameScoreResponse, GameScore>(scores, dataContext);
        return ret;
    }

    [ApiV3Endpoint("scores/{uuid}"), Authentication(false)]
    [DocSummary("Gets an individual score by a UUID")]
    [DocError(typeof(ApiNotFoundError), "The score could not be found")]
    public ApiResponse<ApiGameScoreResponse> GetScoreByUuid(RequestContext context, GameDatabaseContext database,
        DataContext dataContext,
        [DocSummary("The UUID of the score")] string uuid)
    {
        GameScore? score = database.GetScoreByUuid(uuid);
        if (score == null) return ApiNotFoundError.Instance;
        
        return ApiGameScoreResponse.FromOld(score, dataContext);
    }
}