using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Bunkum.HttpServer.Storage;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game.Levels;

public class PublishEndpoints : EndpointGroup
{
    [GameEndpoint("startPublish", ContentType.Xml, Method.Post)]
    public SerializedLevelResources StartPublish(RequestContext context, GameDatabaseContext database, GameLevelResponse body, IDataStore dataStore)
    {
        List<string> hashes = new();
        hashes.AddRange(body.XmlResources);
        hashes.Add(body.RootResource);
        hashes.Add(body.IconHash);

        hashes.RemoveAll(r => r == "0" || r.StartsWith('g') || string.IsNullOrWhiteSpace(r));
        
        return new SerializedLevelResources
        {
            Resources = hashes.Where(r => !dataStore.ExistsInStore(r)).ToArray(),
        };
    }

    [GameEndpoint("publish", ContentType.Xml, Method.Post)]
    public Response PublishLevel(RequestContext context, GameUser user, GameDatabaseContext database, GameLevelResponse body)
    {
        GameLevel level = body.ToGameLevel(user);
        if (level.LevelId != default) // Republish requests contain the id of the old level
        {
            context.Logger.LogInfo(BunkumContext.UserContent, "Republishing level id " + level.LevelId);

            GameLevel? newBody;
            // ReSharper disable once InvertIf
            if ((newBody = database.UpdateLevel(level, user)) != null)
            {
                return new Response(GameLevelResponse.FromOld(newBody)!, ContentType.Xml);
            }
            
            database.AddPublishFailNotification("You may not republish another user's level.", level, user);
            return BadRequest;
        }

        level.Publisher = user;

        database.AddLevel(level);
        database.CreateLevelUploadEvent(user, level);
        
        return new Response(GameLevelResponse.FromOld(level)!, ContentType.Xml);
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