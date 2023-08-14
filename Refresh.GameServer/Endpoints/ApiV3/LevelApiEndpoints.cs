using AttribDoc.Attributes;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Documentation.Attributes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Levels.Categories;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class LevelApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("levels"), Authentication(false)]
    [ClientCacheResponse(86400 / 2)] // cache for half a day
    [DocSummary("Retrieves a list of categories you can use to search levels")]
    [DocQueryParam("includePreviews", "If true, a single level will be added to each category representing a level from that category. False by default.")]
    [DocError(typeof(ApiValidationError), "The boolean 'includePreviews' could not be parsed by the server.")]
    public ApiListResponse<ApiLevelCategoryResponse> GetCategories(RequestContext context, CategoryService categories, MatchService matchService, GameDatabaseContext database, GameUser? user)
    {
        bool result = bool.TryParse(context.QueryString.Get("includePreviews") ?? "false", out bool includePreviews);
        if (!result) return ApiValidationError.BooleanParseError;

        IEnumerable<ApiLevelCategoryResponse> resp;

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (includePreviews)resp = ApiLevelCategoryResponse.FromOldList(categories.Categories, context, matchService, database, user);
        else resp = ApiLevelCategoryResponse.FromOldList(categories.Categories);
        
        return new ApiListResponse<ApiLevelCategoryResponse>(resp);
    }

    [ApiV3Endpoint("levels/{route}"), Authentication(false)]
    [DocSummary("Retrieves a list of levels from a category")]
    [DocError(typeof(ApiNotFoundError), "The level category cannot be found")]
    [DocUsesPageData]
    public ApiListResponse<ApiGameLevelResponse> GetLevels(RequestContext context, GameDatabaseContext database, MatchService matchService, CategoryService categories, GameUser? user,
        [DocSummary("The name of the category you'd like to retrieve levels from. " +
                    "Make a request to /levels to see a list of available categories")] string route)
    {
        (int skip, int count) = context.GetPageData(true);

        DatabaseList<GameLevel>? list = categories.Categories
            .FirstOrDefault(c => c.ApiRoute.StartsWith(route))?
            .Fetch(context, skip, count, matchService, database, user);

        if (list == null) return ApiNotFoundError.Instance;

        return DatabaseList<ApiGameLevelResponse>.FromOldList<ApiGameLevelResponse, GameLevel>(list);
    }

    [ApiV3Endpoint("levels/id/{id}"), Authentication(false)]
    [DocSummary("Gets an individual level by a numerical ID")]
    [DocError(typeof(ApiNotFoundError), "The level cannot be found")]
    public ApiResponse<ApiGameLevelResponse> GetLevelById(RequestContext context, GameDatabaseContext database,
        [DocSummary("The ID of the level")] int id)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.Instance;
        
        return ApiGameLevelResponse.FromOld(level);
    }
}