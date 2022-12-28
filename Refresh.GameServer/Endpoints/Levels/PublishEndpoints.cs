using System.Net;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;
using Refresh.HttpServer;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer.Endpoints.Levels;

public class PublishEndpoints : EndpointGroup
{
    [GameEndpoint("startPublish", ContentType.Xml, Method.Post)]
    public GameResourceLevel StartPublish(RequestContext context, GameUser user, RealmDatabaseContext database, GameLevel body)
    {
        return new GameResourceLevel
        {
            Resources = body.XmlResources,
        };
    }

    [GameEndpoint("publish", ContentType.Xml, Method.Post)]
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