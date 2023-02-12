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
    public GameResourceLevel StartPublish(RequestContext context, RealmDatabaseContext database, GameLevel body)
    {
        List<string> hashes = new();
        hashes.AddRange(body.XmlResources);
        hashes.Add(body.RootResource);
        hashes.Add(body.IconHash);

        hashes.RemoveAll(r => r == "0" || r.StartsWith('g') || string.IsNullOrWhiteSpace(r));
        
        return new GameResourceLevel
        {
            Resources = hashes.Where(r => !context.DataStore.ExistsInStore(r)).ToArray(),
        };
    }

    [GameEndpoint("publish", ContentType.Xml, Method.Post)]
    public Response FinishPublishing(RequestContext context, GameUser user, RealmDatabaseContext database, GameLevel body)
    {
        if (body.LevelId != default) // Republish requests contain the id of the old level
        {
            context.Logger.LogInfo(BunkumContext.UserContent, "Republishing level id " + body.LevelId);

            GameLevel? newBody;
            // ReSharper disable once InvertIf
            if ((newBody = database.UpdateLevel(body, user)) != null)
            {
                newBody.PrepareForSerialization();
                return new Response(newBody, ContentType.Xml);
            }

            return new Response(HttpStatusCode.BadRequest);
        }

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