using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Storage;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Documentation.Attributes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Levels.Categories;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Verification;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class LevelApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("levels"), Authentication(false)]
    [ClientCacheResponse(86400 / 2)] // cache for half a day
    [DocSummary("Retrieves a list of categories you can use to search levels")]
    [DocQueryParam("includePreviews", "If true, a single level will be added to each category representing a level from that category. False by default.")]
    [DocError(typeof(ApiValidationError), "The boolean 'includePreviews' could not be parsed by the server.")]
    public ApiListResponse<ApiLevelCategoryResponse> GetCategories(RequestContext context, CategoryService categories,
        MatchService matchService, GameDatabaseContext database, GameUser? user, IDataStore dataStore,
        DataContext dataContext)
    {
        bool result = bool.TryParse(context.QueryString.Get("includePreviews") ?? "false", out bool includePreviews);
        if (!result) return ApiValidationError.BooleanParseError;

        IEnumerable<ApiLevelCategoryResponse> resp;

        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (includePreviews)resp = ApiLevelCategoryResponse.FromOldListWithExtraData(categories.Categories, context, matchService, database, dataStore, user, dataContext);
        else resp = ApiLevelCategoryResponse.FromOldListWithExtraData(categories.Categories, database, dataStore, dataContext);
        
        return new ApiListResponse<ApiLevelCategoryResponse>(resp);
    }

    [ApiV3Endpoint("levels/{route}"), Authentication(false)]
    [DocSummary("Retrieves a list of levels from a category")]
    [DocError(typeof(ApiNotFoundError), "The level category cannot be found")]
    [DocUsesPageData]
    [DocQueryParam("game", "Filters levels to a specific game version. Allowed values: lbp1-3, vita, psp, beta")]
    [DocQueryParam("seed", "The random seed to use for randomization. Uses 0 if not specified.")]
    [DocQueryParam("players", "Filters levels to those accommodating the specified number of players.")]
    public ApiListResponse<ApiGameLevelResponse> GetLevels(RequestContext context, GameDatabaseContext database,
        MatchService matchService, CategoryService categories, GameUser? user, IDataStore dataStore,
        [DocSummary("The name of the category you'd like to retrieve levels from. " +
                    "Make a request to /levels to see a list of available categories")]
        string route, DataContext dataContext)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            return new ApiError("You didn't specify a route. " +
                                "You probably meant to use the `/levels` endpoint and left a trailing slash in the URL.", NotFound);
        }
        
        (int skip, int count) = context.GetPageData();

        DatabaseList<GameLevel>? list = categories.Categories
            .FirstOrDefault(c => c.ApiRoute.StartsWith(route))?
            .Fetch(context, skip, count, matchService, database, user, new LevelFilterSettings(context, TokenGame.Website), user);

        if (list == null) return ApiNotFoundError.Instance;

        DatabaseList<ApiGameLevelResponse> levels = DatabaseList<ApiGameLevelResponse>.FromOldList<ApiGameLevelResponse, GameLevel>(list, dataContext);
        foreach (ApiGameLevelResponse level in levels.Items)
        {
            level.FillInExtraData(database, dataStore);
        }
        return levels;
    }

    [ApiV3Endpoint("levels/id/{id}"), Authentication(false)]
    [DocSummary("Gets an individual level by a numerical ID")]
    [DocError(typeof(ApiNotFoundError), "The level cannot be found")]
    public ApiResponse<ApiGameLevelResponse> GetLevelById(RequestContext context, GameDatabaseContext database,
        IDataStore dataStore,
        [DocSummary("The ID of the level")] int id, DataContext dataContext)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;
        
        return ApiGameLevelResponse.FromOldWithExtraData(level, database, dataStore, dataContext);
    }
    
    [ApiV3Endpoint("levels/id/{id}", HttpMethods.Patch)]
    [DocSummary("Edits a level by the level's numerical ID")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    [DocError(typeof(ApiAuthenticationError), ApiAuthenticationError.NoPermissionsForObjectWhen)]
    public ApiResponse<ApiGameLevelResponse> EditLevelById(RequestContext context, GameDatabaseContext database,
        GameUser user, IDataStore dataStore,
        [DocSummary("The ID of the level")] int id, ApiEditLevelRequest body, DataContext dataContext)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;
        
        if (level.Publisher?.UserId != user.UserId) 
            return ApiAuthenticationError.NoPermissionsForObject;

        level = database.UpdateLevel(body, level);

        return ApiGameLevelResponse.FromOldWithExtraData(level, database, dataStore, dataContext);
    }

    [ApiV3Endpoint("levels/id/{id}", HttpMethods.Delete)]
    [DocSummary("Deletes a level by the level's numerical ID")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    [DocError(typeof(ApiAuthenticationError), ApiAuthenticationError.NoPermissionsForObjectWhen)]
    public ApiOkResponse DeleteLevelById(RequestContext context, GameDatabaseContext database, GameUser user,
        [DocSummary("The ID of the level")] int id)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;

        if (level.Publisher?.UserId != user.UserId) 
            return ApiAuthenticationError.NoPermissionsForObject;

        database.DeleteLevel(level);

        return new ApiOkResponse();
    }

    [ApiV3Endpoint("levels/id/{id}/setAsOverride", HttpMethods.Post)]
    [DocSummary("Marks the level to show in the next slot list gotten from the game")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiOkResponse SetLevelAsOverrideById(RequestContext context, GameDatabaseContext database, GameUser user, LevelListOverrideService service,
        [DocSummary("The ID of the level")] int id)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;
        
        service.AddIdOverridesForUser(user, level);
        
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("levels/hash/{hash}/setAsOverride", HttpMethods.Post)]
    [DocSummary("Marks the level hash to show in the next slot list gotten from the game")]
    [DocError(typeof(ApiValidationError), ApiValidationError.HashInvalidErrorWhen)]
    public ApiOkResponse SetLevelAsOverrideByHash(RequestContext context, GameDatabaseContext database, GameUser user,
        LevelListOverrideService service, [DocSummary("The hash of level root resource")] string hash)
    {
        if (!CommonPatterns.Sha1Regex().IsMatch(hash)) 
            return ApiValidationError.HashInvalidError;
        
        service.AddHashOverrideForUser(user, hash);
        
        return new ApiOkResponse();
    }
}