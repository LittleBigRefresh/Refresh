using System.Net;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Levels.Categories;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Api;

public class LevelApiEndpoints : EndpointGroup
{
    [ApiEndpoint("levels/{route}")]
    [Authentication(false)]
    [NullStatusCode(HttpStatusCode.NotFound)]
    public IEnumerable<GameLevel>? GetLevels(RequestContext context, RealmDatabaseContext database, GameUser? user, string route) 
        => CategoryHandler.Categories
            .FirstOrDefault(c => c.ApiRoute.StartsWith(route))?
            .Fetch(database, user, 20, 0); // TODO: Implement count/skip for API

    [ApiEndpoint("level/id/{idStr}")]
    [Authentication(false)]
    public GameLevel? GetLevelById(RequestContext context, RealmDatabaseContext database, string idStr)
    {
        int.TryParse(idStr, out int id);
        if (id == default) return null;
        
        return database.GetLevelById(id);
    }
}