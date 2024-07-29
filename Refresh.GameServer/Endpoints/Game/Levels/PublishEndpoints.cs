using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.Common.Constants;
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
    /// <param name="dataContext">The data context associated with the request</param>
    /// <returns>Whether or not validation succeeded</returns>
    private static bool VerifyLevel(GameLevelRequest body, DataContext dataContext)
    {
        if (body.Title.Length > UgcConstantLimits.TitleLimit) 
            body.Title = body.Title[..UgcConstantLimits.TitleLimit];

        if (body.Description.Length > UgcConstantLimits.DescriptionLimit)
            body.Description = body.Description[..UgcConstantLimits.DescriptionLimit];
            
        if (body.MaxPlayers is > 4 or < 0 || body.MinPlayers is > 4 or < 0)
        {
            return false;
        }

        //If the icon hash is a GUID hash, verify its a valid texture GUID
        if (body.IconHash.StartsWith('g') && !dataContext.GuidChecker.IsTextureGuid(dataContext.Game, long.Parse(body.IconHash.AsSpan()[1..]))) 
            return false;

        GameLevel? existingLevel = dataContext.Database.GetLevelByRootResource(body.RootResource);
        // If there is an existing level with this root hash, and this isn't an update request, block the upload
        if (existingLevel != null && body.LevelId != existingLevel.LevelId)
        {
            dataContext.Database.AddPublishFailNotification("The level you tried to publish has already been uploaded by another user.", body.ToGameLevel(dataContext.User!), dataContext.User!);

            return false;
        }

        return true;
    }

    [GameEndpoint("startPublish", ContentType.Xml, HttpMethods.Post)]
    [NullStatusCode(BadRequest)]
    [RequireEmailVerified]
    public SerializedLevelResources? StartPublish(RequestContext context,
        GameLevelRequest body,
        CommandService command,
        DataContext dataContext)
    {
        //If verifying the request fails, return null
        if (!VerifyLevel(body, dataContext)) return null;

        List<string> hashes =
        [
            .. body.XmlResources,
            body.RootResource,
            body.IconHash
        ];

        //Remove all invalid or GUID assets
        hashes.RemoveAll(r => r == "0" || r.StartsWith('g') || string.IsNullOrWhiteSpace(r));

        //Verify all hashes are valid SHA1 hashes
        if (hashes.Any(hash => !CommonPatterns.Sha1Regex().IsMatch(hash))) return null;

        //Mark the user as publishing
        command.StartPublishing(dataContext.User!.UserId);

        return new SerializedLevelResources
        {
            Resources = hashes.Where(r => !dataContext.DataStore.ExistsInStore(r)).ToArray(),
        };
    }

    [GameEndpoint("publish", ContentType.Xml, HttpMethods.Post)]
    [RequireEmailVerified]
    public Response PublishLevel(RequestContext context,
        GameLevelRequest body,
        CommandService commandService,
        DataContext dataContext)
    {
        //If verifying the request fails, return BadRequest
        if (!VerifyLevel(body, dataContext)) return BadRequest;

        GameUser user = dataContext.User!;
        
        GameLevel level = body.ToGameLevel(user);
        level.GameVersion = dataContext.Token!.TokenGame;

        level.MinPlayers = Math.Clamp(level.MinPlayers, 1, 4);
        level.MaxPlayers = Math.Clamp(level.MaxPlayers, 1, 4);

        string rootResourcePath = context.IsPSP() ? $"psp/{level.RootResource}" : level.RootResource;

        //Check if the root resource is a SHA1 hash
        if (!CommonPatterns.Sha1Regex().IsMatch(level.RootResource)) return BadRequest;
        //Make sure the root resource exists in the data store
        if (!dataContext.DataStore.ExistsInStore(rootResourcePath)) return NotFound;

        if (level.LevelId != default) // Republish requests contain the id of the old level
        {
            context.Logger.LogInfo(BunkumCategory.UserContent, "Republishing level id {0}", level.LevelId);

            GameLevel? newBody;
            // ReSharper disable once InvertIf
            if ((newBody = dataContext.Database.UpdateLevel(level, user)) != null)
            {
                return new Response(GameLevelResponse.FromOld(newBody, dataContext)!, ContentType.Xml);
            }

            dataContext.Database.AddPublishFailNotification("You may not republish another user's level.", level, dataContext.User);
            return BadRequest;
        }

        //Mark the user as no longer publishing
        commandService.StopPublishing(dataContext.User!.UserId);

        level.Publisher = dataContext.User;

        dataContext.Database.AddLevel(level);
        dataContext.Database.CreateLevelUploadEvent(dataContext.User, level);

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