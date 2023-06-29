using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Levels.Categories;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.ApiV2;

public class LevelApiEndpoints : EndpointGroup
{
    [ApiV2Endpoint("levels/{route}")]
    [Authentication(false)]
    [NullStatusCode(NotFound)]
    public IEnumerable<GameLevel>? GetLevels(RequestContext context, GameDatabaseContext database, CategoryService categories, GameUser? user, string route)
    {
        (int skip, int count) = context.GetPageData(true);
        
        return categories.Categories
            .FirstOrDefault(c => c.ApiRoute.StartsWith(route))?
            .Fetch(context, skip, count, database, user);
    }

    [ApiV2Endpoint("levels")]
    [Authentication(false)]
    [ClientCacheResponse(86400 / 2)] // cache for half a day
    public IEnumerable<LevelCategory> GetCategories(RequestContext context, CategoryService categories)
        => categories.Categories;

    [ApiV2Endpoint("level/id/{idStr}")]
    [Authentication(false)]
    public GameLevel? GetLevelById(RequestContext context, GameDatabaseContext database, string idStr)
    {
        int.TryParse(idStr, out int id);
        if (id == default) return null;
        
        return database.GetLevelById(id);
    }
}