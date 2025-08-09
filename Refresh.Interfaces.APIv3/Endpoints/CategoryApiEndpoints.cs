using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Refresh.Core.Configuration;
using Refresh.Core.Types.Categories;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Database.Query;
using Refresh.Interfaces.APIv3.Documentation.Attributes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Categories;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Levels;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;
using Refresh.Interfaces.APIv3.Extensions;

namespace Refresh.Interfaces.APIv3.Endpoints;

public class CategoryApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("levels"), Authentication(false)]
    [ClientCacheResponse(1800)] // cache for half an hour
    [DocSummary("Retrieves a list of categories you can use to search levels")]
    [DocQueryParam("includePreviews", "If true, a single level will be added to each category representing a level from that category. False by default.")]
    [DocError(typeof(ApiValidationError), "The boolean 'includePreviews' could not be parsed by the server.")]
    public ApiListResponse<ApiLevelCategoryResponse> GetLevelCategories(RequestContext context, CategoryService categories,
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
    public ApiListResponse<ApiGameLevelResponse> GetLevels(RequestContext context, CategoryService categories, GameUser? user,
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

        DatabaseList<ApiGameLevelResponse> levels = DatabaseListExtensions.FromOldList<ApiGameLevelResponse, GameLevel>(list, dataContext);
        return levels;
    }

    [ApiV3Endpoint("users"), Authentication(false)]
    [ClientCacheResponse(1800)] // cache for half an hour
    [DocSummary("Retrieves a list of categories you can use to search users. Returns an empty list if the instance doesn't allow showing online users.")]
    [DocQueryParam("includePreviews", "If true, a single user will be added to each category representing a user from that category. False by default.")]
    [DocError(typeof(ApiValidationError), "The boolean 'includePreviews' could not be parsed by the server.")]
    public ApiListResponse<ApiUserCategoryResponse> GetUserCategories(RequestContext context, CategoryService categories,
        DataContext dataContext, GameServerConfig config)
    {
        bool result = bool.TryParse(context.QueryString.Get("includePreviews") ?? "false", out bool includePreviews);
        if (!result) return ApiValidationError.BooleanParseError;

        if (!config.PermitShowingOnlineUsers) return new ApiListResponse<ApiUserCategoryResponse>([]);
        IEnumerable<ApiUserCategoryResponse> resp;

        if (includePreviews) resp = ApiUserCategoryResponse.FromOldList(categories.UserCategories, context, dataContext);
        else resp = ApiUserCategoryResponse.FromOldList(categories.UserCategories, dataContext);

        return new ApiListResponse<ApiUserCategoryResponse>(resp);
    }

    [ApiV3Endpoint("users/{route}"), Authentication(false)]
    [DocSummary("Retrieves a list of users from a category.")]
    [DocError(typeof(ApiNotFoundError), "The user category cannot be found, or the instance does not allow showing online users.")]
    [DocUsesPageData]
    [DocQueryParam("username", "If set, certain categories like 'hearted' will return the related users of " +
                               "the user with this username instead of your own. Optional.")]
    public Response GetUsers(RequestContext context, CategoryService categories, GameUser? user,
        [DocSummary("The name of the category you'd like to retrieve users from. " +
                    "Make a request to /users to see a list of available categories")]
        string route, DataContext dataContext, GameServerConfig config)
    {
        // Bunkum usually routes users/me requests to here aswell, so use this hack to serve those requests properly.
        if (route == "me")
        {
            if (user == null) return ApiAuthenticationError.NotAuthenticated; // Error documented in UserApiEndpoints.GetMyUser()
            return new Response(new ApiResponse<ApiExtendedGameUserResponse>(ApiExtendedGameUserResponse.FromOld(user, dataContext)!), ContentType.Json);
        }

        if (string.IsNullOrWhiteSpace(route))
        {
            return new ApiError("You didn't specify a route. " +
                                "You probably meant to use the `/users` endpoint and left a trailing slash in the URL.", NotFound);
        }

        if (!config.PermitShowingOnlineUsers) return ApiNotFoundError.Instance;
        (int skip, int count) = context.GetPageData();

        DatabaseList<GameUser>? list = categories.UserCategories
            .FirstOrDefault(c => c.ApiRoute.StartsWith(route))?
            .Fetch(context, skip, count, dataContext, user);

        if (list == null) return ApiNotFoundError.Instance;

        ApiListResponse<ApiGameUserResponse> users = DatabaseListExtensions.FromOldList<ApiGameUserResponse, GameUser>(list, dataContext);
        return new Response(users, ContentType.Json);
    }
}