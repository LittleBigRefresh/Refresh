using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Core.Storage;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using NotEnoughLogs;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.DataTypes.Request;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Verification;

namespace Refresh.GameServer.Endpoints.Game.Levels;

public class PublishEndpoints : EndpointGroup
{
    /// <summary>
    /// Does basic verification on a level
    /// </summary>
    /// <param name="body">The level to verify</param>
    /// <param name="user">The user that is attempting to upload</param>
    /// <param name="logger">A logger instance</param>
    /// <param name="guidChecker">The associated GuidCheckerService with the request</param>
    /// <param name="game">The game the level is being submitted from</param>
    /// <returns>Whether or not validation succeeded</returns>
    private static bool VerifyLevel(GameLevelRequest body, GameUser user, Logger logger, GuidCheckerService guidChecker, TokenGame game)
    {
        if (body.Title.Length > 256)
        {
            return false;
        }

        if (body.Description.Length > 4096)
        {
            return false;
        }

        if (body.MaxPlayers is > 4 or < 0 || body.MinPlayers is > 4 or < 0)
        {
            return false;
        }

        //If the icon hash is a GUID hash, verify its a valid texture GUID
        if (body.IconHash.StartsWith('g'))
        {
            if (!guidChecker.IsTextureGuid(game, long.Parse(body.IconHash.AsSpan()[1..])))
                return false;
        }
        
        return true;
    }
    
    [GameEndpoint("startPublish", ContentType.Xml, HttpMethods.Post)]
    [NullStatusCode(BadRequest)]
    public SerializedLevelResources? StartPublish(RequestContext context, GameUser user, GameDatabaseContext database, GameLevelRequest body, CommandService command, IDataStore dataStore, GuidCheckerService guidChecker, Token token)
    {
        //If verifying the request fails, return null
        if (!VerifyLevel(body, user, context.Logger, guidChecker, token.TokenGame)) return null;
        
        List<string> hashes = new();
        hashes.AddRange(body.XmlResources);
        hashes.Add(body.RootResource);
        hashes.Add(body.IconHash);

        //Remove all invalid or GUID assets
        hashes.RemoveAll(r => r == "0" || r.StartsWith('g') || string.IsNullOrWhiteSpace(r));
        
        //Verify all hashes are valid SHA1 hashes
        if (hashes.Any(hash => !CommonPatterns.Sha1Regex().IsMatch(hash))) return null;

        //Mark the user as publishing
        command.StartPublishing(user.UserId);
            
        return new SerializedLevelResources
        {
            Resources = hashes.Where(r => !dataStore.ExistsInStore(r)).ToArray(),
        };
    }

    [GameEndpoint("publish", ContentType.Xml, HttpMethods.Post)]
    public Response PublishLevel(RequestContext context,
        GameUser user,
        Token token,
        GameDatabaseContext database,
        GameLevelRequest body,
        CommandService commandService,
        IDataStore dataStore,
        GuidCheckerService guidChecker, DataContext dataContext)
    {
        //If verifying the request fails, return null
        if (!VerifyLevel(body, user, context.Logger, guidChecker, token.TokenGame)) return BadRequest;
        
        GameLevel level = body.ToGameLevel(user);
        level.GameVersion = token.TokenGame;

        level.MinPlayers = Math.Clamp(level.MinPlayers, 1, 4);
        level.MaxPlayers = Math.Clamp(level.MaxPlayers, 1, 4);

        string rootResourcePath = context.IsPSP() ? $"psp/{level.RootResource}" : level.RootResource;
        
        //Check if the root resource is a SHA1 hash
        if (!CommonPatterns.Sha1Regex().IsMatch(level.RootResource)) return BadRequest;
        //Make sure the root resource exists in the data store
        if (!dataStore.ExistsInStore(rootResourcePath)) return NotFound;

        if (level.LevelId != default) // Republish requests contain the id of the old level
        {
            context.Logger.LogInfo(BunkumCategory.UserContent, "Republishing level id {0}", level.LevelId);

            GameLevel? newBody;
            // ReSharper disable once InvertIf
            if ((newBody = database.UpdateLevel(level, user)) != null)
            {
                return new Response(GameLevelResponse.FromOld(newBody, dataContext)!, ContentType.Xml);
            }
            
            database.AddPublishFailNotification("You may not republish another user's level.", level, user);
            return BadRequest;
        }
        
        //Mark the user as no longer publishing
        commandService.StopPublishing(user.UserId);

        level.Publisher = user;

        database.AddLevel(level);
        database.CreateLevelUploadEvent(user, level);
        
        context.Logger.LogInfo(BunkumCategory.UserContent, "User {0} (id: {1}) uploaded level id {2}", user.Username, user.UserId, level.LevelId);
        
        return new Response(GameLevelResponse.FromOld(level, dataContext)!, ContentType.Xml);
    }

    [GameEndpoint("unpublish/{id}", ContentType.Xml, HttpMethods.Post)]
    public Response DeleteLevel(RequestContext context, GameUser user, GameDatabaseContext database, int id)
    {
        GameLevel? level = database.GetLevelById(id);
        if (level == null) return NotFound;

        if (level.Publisher?.UserId != user.UserId) return Unauthorized;

        database.DeleteLevel(level);
        return OK;
    }
}