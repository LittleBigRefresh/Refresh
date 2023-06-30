using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Documentation.Attributes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Levels.Categories;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class LevelApiEndpoints : EndpointGroup
{
    [ApiV3Endpoint("levels/{route}"), Authentication(false)]
    [DocSummary("Retrieves a list of levels from a category.")]
    [DocError(typeof(ApiNotFoundError), "The level category cannot be found.")]
    [DocUsesPageData]
    public ApiListResponse<GameLevel> GetLevels(RequestContext context, GameDatabaseContext database, CategoryService categories, GameUser? user,
        [DocSummary("The name of the category you'd like to retrieve levels from. " +
                    "Make a request to /levels to see a list of available categories.")] string route)
    {
        (int skip, int count) = context.GetPageData(true);

        DatabaseList<GameLevel>? list = categories.Categories
            .FirstOrDefault(c => c.ApiRoute.StartsWith(route))?
            .Fetch(context, skip, count, database, user);

        if (list == null) return ApiNotFoundError.Instance;
        
        return list;
    }

    [ApiV3Endpoint("levels"), Authentication(false)]
    [ClientCacheResponse(86400 / 2)] // cache for half a day
    [DocSummary("Retrieves a list of categories you can use to search levels.")]
    public ApiListResponse<LevelCategory> GetCategories(RequestContext context, CategoryService categories)
        => new(categories.Categories);

    [ApiV3Endpoint("level/id/{id}"), Authentication(false)]
    [DocSummary("Gets an individual level by a numerical ID.")]
    [DocError(typeof(ApiNotFoundError), "The level cannot be found.")]
    public GameLevel? GetLevelById(RequestContext context, GameDatabaseContext database,
        [DocSummary("The ID of the level you would like to request.")] int id)
        => database.GetLevelById(id);
}