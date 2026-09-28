using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.RateLimit;
using Bunkum.Core.Storage;
using Refresh.Core.RateLimits.Leaderboard;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Documentation.Attributes;
using Refresh.Interfaces.APIv3.Documentation.Descriptions;
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
    [RateLimitSettings(ScoreListEndpointLimits.TimeoutDuration, ScoreListEndpointLimits.ApiRequestAmount, 
                                ScoreListEndpointLimits.BlockDuration, ScoreListEndpointLimits.ApiRequestBucket)]
    public ApiListResponse<ApiGameScoreResponse> GetTopScoresForLevel(RequestContext context,
        GameDatabaseContext database, IDataStore dataStore,
        [DocSummary("The ID of the level")] int id,
        [DocSummary("The leaderboard mode (aka the number of players, e.g. 2 for 2-player mode)")]
        int mode, DataContext dataContext)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;
        
        (int skip, int count) = context.GetPageData();

        bool result = bool.TryParse(context.QueryString.Get("showAll") ?? "false", out bool showAll);
        if (!result) return ApiValidationError.BooleanParseError;

        // Don't have type 7 break on APIv3 clients which happen to already use it
        byte scoreType = (byte)(mode == 7 ? 0 : mode);

        DatabaseList<ScoreWithRank> scores = database.GetTopScoresForLevel(level, count, skip, scoreType, showAll);
        DatabaseList<ApiGameScoreResponse> ret = DatabaseListExtensions.FromOldList<ApiGameScoreResponse, ScoreWithRank>(scores, dataContext);
        return ret;
    }
    
    [ApiV3Endpoint("users/{idType}/{userId}/scores"), Authentication(false)]
    [DocUsesPageData, DocSummary("Gets a list of the top scores on a level.")]
    // TODO i really can't wait to move all these strings to their own place, this looks really messy (like many other API endpoints)
    [DocQueryParam("showAll", "Whether or not to show all scores. If false, only the user's best scores per level/mode will be shown." +
                              "If true, all scores will be shown no matter what. False by default.")]
    [DocQueryParam("mode", "The leaderboard mode (aka the number of players, e.g. 2 for 2-player mode)." +
                              "If 0 or missing, scores won't be filtered by mode.")]
    [DocQueryParam("minRank", "The minimum rank all scores have to be at (among their levels' main leaderboard)." +
                              "For example, minRank 1 and maxRank 3 will only keep all scores which are currently rank 1, 2 or 3." +
                              "If 0 or missing, scores won't be filtered by minimum rank.")]
    [DocQueryParam("maxRank", "The maximum rank all scores have to be at (among their levels' main leaderboard)." +
                              "For example, minRank 1 and maxRank 3 will only keep all scores which are currently rank 1, 2 or 3." +
                              "If 0 or missing, scores won't be filtered by minimum rank.")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.UserMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), "The boolean 'showAll' could not be parsed by the server.")]
    [DocError(typeof(ApiNotFoundError), ApiValidationError.ScoreModeInvalidErrorWhen)]
    [DocError(typeof(ApiNotFoundError), ApiValidationError.ScoreRanksInvalidErrorWhen)]
    public ApiListResponse<ApiGameScoreResponse> GetTopScoresByUser(RequestContext context,
        [DocSummary(SharedParamDescriptions.UserIdParam)] string userId,
        [DocSummary(SharedParamDescriptions.UserIdTypeParam)] string idType,
        DataContext dataContext)
    {
        GameUser? user = dataContext.Database.GetUserByIdAndType(idType, userId);
        if(user == null) return ApiNotFoundError.UserMissingError;
        
        (int skip, int count) = context.GetPageData();

        // TODO "showAll" is not very clear, rename it for both endpoints in APIv4
        bool result = bool.TryParse(context.QueryString.Get("showAll") ?? "false", out bool showAll);
        if (!result) return ApiValidationError.BooleanParseError;
        
        bool modeParsed = byte.TryParse(context.QueryString.Get("mode") ?? "0", out byte mode);
        if (!modeParsed || mode > 4) return ApiValidationError.ScoreModeInvalidError;

        bool minRankParsed = uint.TryParse(context.QueryString.Get("minRank") ?? "0", out uint minRank);
        bool maxRankParsed = uint.TryParse(context.QueryString.Get("maxRank") ?? "0", out uint maxRank);
        // Don't think it's necessary to check whether min is higher than max.
        // Could be friendly and warn the client instead of just returning an empty list, but that's just unnecessary
        // extra work for us for nearly no reason, so whatever.
        if (!minRankParsed || !maxRankParsed) return ApiValidationError.ScoreRanksInvalidError;

        DatabaseList<ScoreWithRank> scores = dataContext.Database.GetTopScoresByUser(user, count, skip, mode, minRank, maxRank, showAll);
        DatabaseList<ApiGameScoreResponse> ret = DatabaseListExtensions.FromOldList<ApiGameScoreResponse, ScoreWithRank>(scores, dataContext);
        return ret;
    }

    [ApiV3Endpoint("scores/{uuid}"), Authentication(false)]
    [DocSummary("Gets an individual score by a UUID")]
    [DocError(typeof(ApiNotFoundError), "The score could not be found")]
    [RateLimitSettings(SingleScoreEndpointLimits.TimeoutDuration, SingleScoreEndpointLimits.RequestAmount, 
                                SingleScoreEndpointLimits.BlockDuration, SingleScoreEndpointLimits.RequestBucket)]
    public ApiResponse<ApiGameScoreResponse> GetScoreByUuid(RequestContext context, GameDatabaseContext database,
        DataContext dataContext,
        [DocSummary("The UUID of the score")] string uuid)
    {
        GameScore? score = database.GetScoreByUuid(uuid);
        if (score == null) return ApiNotFoundError.Instance;
        
        return ApiGameScoreResponse.FromOld(score, dataContext);
    }
}