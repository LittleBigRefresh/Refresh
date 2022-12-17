using System.Net;
using System.Xml.Serialization;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
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
    public GameResourceLevel? StartPublish(RequestContext context, GameUser user, RealmDatabaseContext database, GameLevel body)
    {
        return new GameResourceLevel
        {
            Resources = body.XmlResources,
        };
    }

    [GameEndpoint("publish", ContentType.Xml)]
    public Response FinishPublishing(RequestContext context, GameUser user, RealmDatabaseContext database, GameLevel body)
    {
        body.Publisher = user;
        
        // ReSharper disable once InvertIf
        if (database.AddLevel(body))
        {
            body.PrepareForSerialization();
            return new Response(body, ContentType.Xml);
        }

        return new Response(HttpStatusCode.BadRequest);
    }
}