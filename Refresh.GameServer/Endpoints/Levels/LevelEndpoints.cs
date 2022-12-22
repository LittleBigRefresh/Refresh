using System.Net;
using JetBrains.Annotations;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.UserData;
using Refresh.HttpServer;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer.Endpoints.Levels;

public class LevelEndpoints : EndpointGroup
{
    [GameEndpoint("slots/by", ContentType.Xml)]
    [NullStatusCode(HttpStatusCode.NotFound)]
    public GameMinimalLevelList? LevelsByUser(RequestContext context, RealmDatabaseContext database)
    {
        GameUser? user = database.GetUser(context.Request.QueryString["u"]);
        if (user == null) return null;

        (int skip, int count) = this.GetPageData(context);

        IEnumerable<GameMinimalLevel> list = database.GetLevelsByUser(user, count, skip)
            .Select(GameMinimalLevel.FromGameLevel);

        return new GameMinimalLevelList(list, database.GetTotalLevelCount());
    }

    [GameEndpoint("slots", ContentType.Xml)]
    public GameMinimalLevelList NewestLevels(RequestContext context, RealmDatabaseContext database)
    {
        (int skip, int count) = this.GetPageData(context);
        
        IEnumerable<GameMinimalLevel> list = database.GetNewestLevels(count, skip)
            .Select(GameMinimalLevel.FromGameLevel);
        
        return new GameMinimalLevelList(list, database.GetTotalLevelCount());
    }

    [GameEndpoint("s/user/{idStr}", ContentType.Xml)]
    [NullStatusCode(HttpStatusCode.NotFound)]
    public GameLevel? LevelById(RequestContext context, RealmDatabaseContext database, string idStr)
    {
        int.TryParse(idStr, out int id);
        if (id == default) return null;
        
        return database.GetLevelById(id);
    }

    [Pure]
    private (int, int) GetPageData(RequestContext context)
    {
        int.TryParse(context.Request.QueryString["pageStart"], out int skip);
        if (skip != default) skip--;
        
        int.TryParse(context.Request.QueryString["pageSize"], out int count);
        if (count == default) count = 20;

        return (skip, count);
    }
}