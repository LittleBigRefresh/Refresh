using System.Net;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Levels.Categories;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game.Levels;

public class LevelEndpoints : EndpointGroup
{
    // FIXME: Workaround shitty routing - see https://github.com/LittleBigRefresh/Refresh/pull/13#discussion_r1086131790 for details
    [GameEndpoint("slots", ContentType.Xml)]
    public GameMinimalLevelList NewestLevels(RequestContext context, RealmDatabaseContext database, GameUser? user) 
        => this.GetLevels(context, database, user, "newest");

    [GameEndpoint("slots/{route}", ContentType.Xml)]
    public GameMinimalLevelList GetLevels(RequestContext context, RealmDatabaseContext database, GameUser? user, string route) =>
        new(CategoryHandler.Categories
            .FirstOrDefault(c => c.GameRoute.StartsWith(route))?
            .Fetch(context, database, user)?
            .Select(GameMinimalLevel.FromGameLevel), database.GetTotalLevelCount()); // TODO: proper level count

    [GameEndpoint("s/user/{idStr}", ContentType.Xml)]
    [NullStatusCode(HttpStatusCode.NotFound)]
    public GameLevel? LevelById(RequestContext context, RealmDatabaseContext database, string idStr)
    {
        int.TryParse(idStr, out int id);
        if (id == default) return null;
        
        return database.GetLevelById(id);
    }
}