using Refresh.GameServer.Database;
using Refresh.GameServer.Types;
using Refresh.GameServer.Types.UserData;
using Refresh.HttpServer;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer.Endpoints;

public class LevelEndpoints : EndpointGroup
{
    [GameEndpoint("slots/by", ContentType.Xml)]
    public string SlotsByUser(RequestContext context)
    {
        return "<slots total=\"0\" hint_start=\"0\"></slots>";
    }

    [GameEndpoint("startPublish", ContentType.Xml)]
    public GameLevel StartPublish(RequestContext context, GameUser user, RealmDatabaseContext database, string body)
    {
        Console.WriteLine(body);
        return new GameLevel();
    }
}