using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Endpoints.Api;

public class LevelApiEndpoints : EndpointGroup
{
    // TODO: System for level categories so they can be unified
    [ApiEndpoint("levels/newest")]
    [Authentication(false)]
    public IEnumerable<GameLevel> GetLevelsByNewest(RequestContext context, RealmDatabaseContext database)
        => database.GetNewestLevels(20, 0);

    [ApiEndpoint("level/id/{idStr}")]
    [Authentication(false)]
    public GameLevel? GetLevelById(RequestContext context, RealmDatabaseContext database, string idStr)
    {
        int.TryParse(idStr, out int id);
        if (id == default) return null;
        
        return database.GetLevelById(id);
    }
}