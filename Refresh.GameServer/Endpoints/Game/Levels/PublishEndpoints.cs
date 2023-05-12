using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Bunkum.HttpServer.Storage;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game.Levels;

public class PublishEndpoints : EndpointGroup
{
    [GameEndpoint("startPublish", ContentType.Xml, Method.Post)]
    public GameResourceLevel StartPublish(RequestContext context, GameDatabaseContext database, GameLevel body, IDataStore dataStore)
    {
        List<string> hashes = new();
        hashes.AddRange(body.XmlResources);
        hashes.Add(body.RootResource);
        hashes.Add(body.IconHash);

        hashes.RemoveAll(r => r == "0" || r.StartsWith('g') || string.IsNullOrWhiteSpace(r));
        
        return new GameResourceLevel
        {
            Resources = hashes.Where(r => !dataStore.ExistsInStore(r)).ToArray(),
        };
    }

    [GameEndpoint("publish", ContentType.Xml, Method.Post)]
    public Response PublishLevel(RequestContext context, GameUser user, GameDatabaseContext database, GameLevel body)
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

            return BadRequest;
        }

        body.Publisher = user;

        if (!database.AddLevel(body)) return BadRequest;

        database.CreateLevelUploadEvent(user, body);
            
        body.PrepareForSerialization();
        return new Response(body, ContentType.Xml);
    }

    [GameEndpoint("unpublish/{idStr}", ContentType.Xml, Method.Post)]
    public Response DeleteLevel(RequestContext context, GameUser user, GameDatabaseContext database, string idStr)
    {
        int.TryParse(idStr, out int id);
        if (id == default) return BadRequest;

        GameLevel? level = database.GetLevelById(id);
        if (level == null) return NotFound;

        if (level.Publisher?.UserId != user.UserId) return Unauthorized;

        database.DeleteLevel(level);
        return OK;
    }
}