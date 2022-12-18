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
    public GameMinimalLevelList? SlotsByUser(RequestContext context, RealmDatabaseContext database)
    {
        GameUser? user = database.GetUser(context.Request.QueryString["u"]);
        if (user == null) return null;
        
        int.TryParse(context.Request.QueryString["pageStart"], out int skip);
        if (skip != default) skip--;
        
        int.TryParse(context.Request.QueryString["pageSize"], out int count);
        if (count == default) count = 20;

        IEnumerable<GameMinimalLevel> list = database.GetLevelsByUser(user, count, skip)
            .Select(GameMinimalLevel.FromGameLevel);

        return new GameMinimalLevelList(list, database.GetTotalLevelCount());
    }
}