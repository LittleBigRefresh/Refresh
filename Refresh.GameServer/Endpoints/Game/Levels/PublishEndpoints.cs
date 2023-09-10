using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Bunkum.HttpServer.Storage;
using NotEnoughLogs;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.DataTypes.Request;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game.Levels;

public class PublishEndpoints : EndpointGroup
{
    /// <summary>
    /// Does basic verification on a level
    /// </summary>
    /// <param name="body">The level to verify</param>
    /// <param name="user">The user that is attempting to upload</param>
    /// <param name="logger">A logger instance</param>
    /// <returns>Whether or not validation succeeded</returns>
    private static bool VerifyLevel(GameLevelRequest body, GameUser user, LoggerContainer<BunkumContext> logger)
    {
        if (body.Title.Length > 256)
        {
            return false;
        }

        if (body.Description.Length > 4096)
        {
            return false;
        }

        if (body.MaxPlayers > 4 || body.MinPlayers > 4)
        {
            return false;
        }

        return true;
    }
    
    [GameEndpoint("startPublish", ContentType.Xml, Method.Post)]
    [NullStatusCode(BadRequest)]
    public SerializedLevelResources? StartPublish(RequestContext context, GameUser user, GameDatabaseContext database, GameLevelRequest body, CommandService command, IDataStore dataStore)
    {
        //If verifying the request fails, return null
        if (!VerifyLevel(body, user, context.Logger)) return null;
        
        List<string> hashes = new();
        hashes.AddRange(body.XmlResources);
        hashes.Add(body.RootResource);
        hashes.Add(body.IconHash);

        hashes.RemoveAll(r => r == "0" || r.StartsWith('g') || string.IsNullOrWhiteSpace(r));
        
        if (hashes.Any(hash => hash.Length != 40)) return null;

        //Mark the user as publishing
        command.StartPublishing(user.UserId);
            
        return new SerializedLevelResources
        {
            Resources = hashes.Where(r => !dataStore.ExistsInStore(r)).ToArray(),
        };
    }

    [GameEndpoint("publish", ContentType.Xml, Method.Post)]
    public Response PublishLevel(RequestContext context, GameUser user, Token token, GameDatabaseContext database, GameLevelRequest body, CommandService command, IDataStore dataStore)
    {
        //If verifying the request fails, return null
        if (!VerifyLevel(body, user, context.Logger)) return BadRequest;

        GameLevel level = body.ToGameLevel(user);
        level.GameVersion = token.TokenGame;

        level.MinPlayers = Math.Clamp(level.MinPlayers, 1, 4);
        level.MaxPlayers = Math.Clamp(level.MaxPlayers, 1, 4);

        string rootResourcePath = context.IsPSP() ? $"psp/{level.RootResource}" : level.RootResource;
        
        if (level.RootResource.Length != 40) return BadRequest;
        if (!dataStore.ExistsInStore(rootResourcePath)) return NotFound;

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
        
        //Mark the user as no longer publishing
        command.StopPublishing(user.UserId);

        level.Publisher = user;

        database.AddLevel(level);
        database.CreateLevelUploadEvent(user, level);
        
        return new Response(GameLevelResponse.FromOld(level)!, ContentType.Xml);
    }

    [GameEndpoint("unpublish/{id}", ContentType.Xml, Method.Post)]
    public Response DeleteLevel(RequestContext context, GameUser user, GameDatabaseContext database, int id)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return NotFound;

        if (level.Publisher?.UserId != user.UserId) return Unauthorized;

        database.DeleteLevel(level);
        return OK;
    }
}