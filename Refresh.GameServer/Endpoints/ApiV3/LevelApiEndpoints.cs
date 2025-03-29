using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Storage;
using Bunkum.Protocols.Http;
using Refresh.Common.Verification;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Documentation.Attributes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Levels;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.Categories;

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
        if (includePreviews) resp = ApiLevelCategoryResponse.FromOldList(categories.LevelCategories, context, dataContext);
        else resp = ApiLevelCategoryResponse.FromOldList(categories.LevelCategories, dataContext);
        
        return new ApiListResponse<ApiLevelCategoryResponse>(resp);
    }

    [ApiV3Endpoint("levels/{route}"), Authentication(false)]
    [DocSummary("Retrieves a list of levels from a category")]
    [DocError(typeof(ApiNotFoundError), "The level category cannot be found")]
    [DocUsesPageData]
    [DocQueryParam("game", "Filters levels to a specific game version. Allowed values: lbp1-3, vita, psp, beta")]
    [DocQueryParam("seed", "The random seed to use for randomization. Uses 0 if not specified.")]
    [DocQueryParam("players", "Filters levels to those accommodating the specified number of players.")]
    [DocQueryParam("username", "If set, certain categories like 'hearted' or 'byUser' will return the levels of " + 
                               "the user with this username instead of your own. Optional.")]
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

        DatabaseList<GameLevel>? list = categories.LevelCategories
            .FirstOrDefault(c => c.ApiRoute.StartsWith(route))?
            .Fetch(context, skip, count, dataContext, new LevelFilterSettings(context, TokenGame.Website), user);

        if (list == null) return ApiNotFoundError.Instance;

        DatabaseList<ApiGameLevelResponse> levels = DatabaseList<ApiGameLevelResponse>.FromOldList<ApiGameLevelResponse, GameLevel>(list, dataContext);
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
        
        return ApiGameLevelResponse.FromOld(level, dataContext);
    }
    
    [ApiV3Endpoint("levels/hash/{hash}"), Authentication(false)]
    [DocSummary("Gets an individual level by the level's RootResource hash")]
    [DocError(typeof(ApiNotFoundError), "The level cannot be found")]
    public ApiResponse<ApiGameLevelResponse> GetLevelByRootResource(RequestContext context, GameDatabaseContext database,
        IDataStore dataStore,
        [DocSummary("The RootResource hash of the level")] string hash, DataContext dataContext)
    {
        GameLevel? level = database.GetLevelByRootResource(hash);
        if (level == null) return ApiNotFoundError.LevelMissingError;
        
        return ApiGameLevelResponse.FromOld(level, dataContext);
    }
    
    [ApiV3Endpoint("levels/id/{id}", HttpMethods.Patch)]
    [DocSummary("Edits a level by the level's numerical ID")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    [DocError(typeof(ApiAuthenticationError), ApiAuthenticationError.NoPermissionsForObjectWhen)]
    public ApiResponse<ApiGameLevelResponse> EditLevelById(RequestContext context,
        [DocSummary("The ID of the level")] int id, ApiEditLevelRequest body, DataContext dataContext)
    {
        GameLevel? level = dataContext.Database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;
        
        if (level.Publisher?.UserId != dataContext.User!.UserId) 
            return ApiAuthenticationError.NoPermissionsForObject;

        if (body.IconHash != null && body.IconHash.StartsWith('g') &&
            !dataContext.GuidChecker.IsTextureGuid(level.GameVersion, long.Parse(body.IconHash)))
            return ApiValidationError.InvalidTextureGuidError;
        
        level = dataContext.Database.UpdateLevel(body, level);

        return ApiGameLevelResponse.FromOld(level, dataContext);
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
    public ApiOkResponse SetLevelAsOverrideById(RequestContext context, 
        GameDatabaseContext database, 
        GameUser user, 
        PlayNowService overrideService,
        [DocSummary("The ID of the level")] int id)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;

        // TODO: return whether or not the presence server was used
        overrideService.PlayNowLevel(user, level);
        
        return new ApiOkResponse();
    }
    
    [ApiV3Endpoint("levels/hash/{hash}/setAsOverride", HttpMethods.Post)]
    [DocSummary("Marks the level hash to show in the next slot list gotten from the game")]
    [DocError(typeof(ApiValidationError), ApiValidationError.HashInvalidErrorWhen)]
    public ApiOkResponse SetLevelAsOverrideByHash(RequestContext context, GameDatabaseContext database, GameUser user,
        PlayNowService service, PresenceService presenceService, [DocSummary("The hash of level root resource")] string hash)
    {
        if (!CommonPatterns.Sha1Regex().IsMatch(hash)) 
            return ApiValidationError.HashInvalidError;

        // TODO: return whether presence/hash play now was used
        service.PlayNowHash(user, hash);
        
        return new ApiOkResponse();
    }


    [ApiV3Endpoint("levels/id/{id}/relations"), MinimumRole(GameUserRole.Restricted)]
    [DocSummary("Gets your relations to a level by it's ID")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiResponse<ApiGameLevelRelationsResponse> GetLevelRelationsOfUser(RequestContext context, GameDatabaseContext database, GameUser user,
        [DocSummary("The ID of the level")] int id)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;

        return new ApiGameLevelRelationsResponse
        {
            IsHearted = database.IsLevelFavouritedByUser(level, user),
            IsQueued = database.IsLevelQueuedByUser(level, user),
            MyPlaysCount = database.GetTotalPlaysForLevelByUser(level, user)
        };
    }

    [ApiV3Endpoint("levels/id/{id}/heart", HttpMethods.Post)]
    [DocSummary("Adds a specific level by it's ID to your hearted levels")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiOkResponse FavouriteLevel(RequestContext context, GameDatabaseContext database, GameUser user,
        [DocSummary("The ID of the level")] int id) 
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;

        database.FavouriteLevel(level, user);
        return new ApiOkResponse();
    }

    [ApiV3Endpoint("levels/id/{id}/unheart", HttpMethods.Post)]
    [DocSummary("Removes a specific level by it's ID from your hearted levels")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiOkResponse UnheartLevel(RequestContext context, GameDatabaseContext database, GameUser user,
        [DocSummary("The ID of the level")] int id) 
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;

        database.UnfavouriteLevel(level, user);
        return new ApiOkResponse();
    }

    [ApiV3Endpoint("levels/id/{id}/queue", HttpMethods.Post)]
    [DocSummary("Adds a specific level by it's ID to your queue")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiOkResponse QueueLevel(RequestContext context, GameDatabaseContext database, GameUser user,
        [DocSummary("The ID of the level")] int id) 
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;

        database.QueueLevel(level, user);
        return new ApiOkResponse();
    }

    [ApiV3Endpoint("levels/id/{id}/dequeue", HttpMethods.Post)]
    [DocSummary("Removes a specific level by it's ID from your queue")]
    [DocError(typeof(ApiNotFoundError), ApiNotFoundError.LevelMissingErrorWhen)]
    public ApiOkResponse DequeueLevel(RequestContext context, GameDatabaseContext database, GameUser user,
        [DocSummary("The ID of the level")] int id) 
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return ApiNotFoundError.LevelMissingError;

        database.DequeueLevel(level, user);
        return new ApiOkResponse();
    }

    [ApiV3Endpoint("levels/queued/clear", HttpMethods.Post)]
    [DocSummary("Clears your level queue")]
    public ApiOkResponse ClearQueuedLevels(RequestContext context, GameDatabaseContext database,
        IDataStore dataStore, GameUser user, DataContext dataContext) 
    {
        database.ClearQueue(user);
        return new ApiOkResponse();
    }
}