using System.Net;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game.Levels;

public class PublishEndpoints : EndpointGroup
{
    [GameEndpoint("startPublish", ContentType.Xml, Method.Post)]
    public GameResourceLevel StartPublish(RequestContext context, GameUser user, RealmDatabaseContext database, GameLevel body) 
        => new()
        {
            Resources = body.XmlResources.Where(r => !context.DataStore.ExistsInStore(r)).ToArray(),
        };

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