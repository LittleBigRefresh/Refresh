using System.Net;
using JetBrains.Annotations;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
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

        (int skip, int count) = context.GetPageData();

        IEnumerable<GameMinimalLevel> list = database.GetLevelsByUser(user, count, skip)
            .Select(GameMinimalLevel.FromGameLevel);

        return new GameMinimalLevelList(list, database.GetTotalLevelCount());
    }

    [GameEndpoint("slots", ContentType.Xml)]
    public GameMinimalLevelList NewestLevels(RequestContext context, RealmDatabaseContext database)
    {
        (int skip, int count) = context.GetPageData();
        
        IEnumerable<GameMinimalLevel> list = database.GetNewestLevels(count, skip)
            .Select(GameMinimalLevel.FromGameLevel);
        
        return new GameMinimalLevelList(list, database.GetTotalLevelCount());
    }

    [GameEndpoint("slots/search", ContentType.Xml)]
    [Authentication(false)]
    public GameMinimalLevelList SearchForLevels(RequestContext context, RealmDatabaseContext database)
    {
        (int skip, int count) = context.GetPageData();
        string? query = context.Request.QueryString["query"];
        if (query == null) return new GameMinimalLevelList();

        (IEnumerable<GameLevel>? list, int totalResults) = database.SearchForLevels(count, skip, query);

        return new GameMinimalLevelList(list.Select(GameMinimalLevel.FromGameLevel), totalResults);
    }

    [GameEndpoint("s/user/{idStr}", ContentType.Xml)]
    [NullStatusCode(HttpStatusCode.NotFound)]
    public GameLevel? LevelById(RequestContext context, RealmDatabaseContext database, string idStr)
    {
        int.TryParse(idStr, out int id);
        if (id == default) return null;
        
        return database.GetLevelById(id);
    }
}