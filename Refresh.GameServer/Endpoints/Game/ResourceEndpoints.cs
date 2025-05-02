using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Core.Storage;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.Common.Verification;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Importing;
using Refresh.GameServer.Services;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.Assets;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Game;

public class ResourceEndpoints : EndpointGroup
{
    //NOTE: type does nothing here, but it's sent by LBP so we have to accept it
    [GameEndpoint("upload/{hash}/{type}", HttpMethods.Post)]
    [GameEndpoint("upload/{hash}", HttpMethods.Post)]
    [RequireEmailVerified]
    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    public Response UploadAsset(RequestContext context, string hash, string type, byte[] body, IDataStore dataStore,
        GameDatabaseContext database, GameUser user, AssetImporter importer, GameServerConfig config, IDateTimeProvider timeProvider, Token token)
    {
        if (user.IsWriteBlocked(config))
            return Unauthorized;
        
        if (!CommonPatterns.Sha1Regex().IsMatch(hash)) return BadRequest;
        
        bool isPSP = context.IsPSP();
        string assetPath = hash;
        if (isPSP)
            assetPath = $"psp/{hash}";

        if (dataStore.ExistsInStore(assetPath))
            return Conflict;
        
        if (body.Length + user.FilesizeQuotaUsage > config.UserFilesizeQuota)
        {
            context.Logger.LogWarning(BunkumCategory.UserContent, "User {0} has hit the filesize quota ({1} bytes), rejecting.", user.Username, config.UserFilesizeQuota);
            return RequestEntityTooLarge;
        }
        
        if (body.Length > 1_048_576 * 2)
        {
            context.Logger.LogWarning(BunkumCategory.UserContent, "{0} is above 2MB ({1} bytes), rejecting.", hash, body.Length);
            return RequestEntityTooLarge;
        }
        
        GameAsset? gameAsset = importer.ReadAndVerifyAsset(hash, body, token.TokenPlatform, database);
        if (gameAsset == null)
            return BadRequest;

        gameAsset.UploadDate = DateTimeOffset.FromUnixTimeSeconds(Math.Clamp(gameAsset.UploadDate.ToUnixTimeSeconds(), timeProvider.EarliestDate, timeProvider.TimestampSeconds));

        AssetFlags blockedAssetFlags = config.BlockedAssetFlags.ToAssetFlags();
        if (user.Role >= GameUserRole.Trusted)
            blockedAssetFlags = config.BlockedAssetFlagsForTrustedUsers.ToAssetFlags();
       
        // Don't block any assets uploaded from PSP, else block any unwanted assets,
        // For example, if the "blocked asset flags" has the "Media" bit set, and so does the asset,
        // then that bit will be set after the AND operation, and we know to block it.
        if ((gameAsset.AssetFlags & blockedAssetFlags) != 0 && !isPSP)
        {
            context.Logger.LogWarning(BunkumCategory.UserContent, $"{gameAsset.AssetType} {hash} by {user} is above configured safety limit " +
                                                                 $"({gameAsset.AssetFlags} is blocked by {blockedAssetFlags})");
            return Unauthorized;
        }

        if (isPSP && gameAsset.AssetFlags.HasFlag(AssetFlags.Media) && blockedAssetFlags.HasFlag(AssetFlags.Media))
        {
            context.Logger.LogWarning(BunkumCategory.UserContent, $"{gameAsset.AssetType} {hash} by {user} cannot be uploaded because media is disabled");
            return Unauthorized;
        }

        if (!dataStore.WriteToStore(assetPath, body))
            return InternalServerError;

        gameAsset.OriginalUploader = user;
        database.AddAssetToDatabase(gameAsset);
        
        database.IncrementUserFilesizeQuota(user, body.Length);

        GameLevel? level = database.GetLevelByRootResource(hash);
        // If there is a level with this root resource, update the modded status of the level
        // This is catching the case where the level resource was uploaded after the slot was published.
        if (level != null) database.UpdateLevelModdedStatus(level);
        
        context.Logger.LogInfo(BunkumCategory.UserContent, $"{user} uploaded a {gameAsset.AssetType} ({body.Length / 1024f:F1} KB)");
        return OK;
    }

    [GameEndpoint("r/{hash}")]
    [MinimumRole(GameUserRole.Restricted)]
    public Response GetResource(RequestContext context, GameUser user, string hash, DataContext dataContext, ChallengeGhostRateLimitService ghostService)
    {
        if (!CommonPatterns.Sha1Regex().IsMatch(hash)) return BadRequest;
        
        //If the request comes from a PSP client,
        if (context.IsPSP())
        {
            //Point the hash into the `psp` folder
            hash = $"psp/{hash}";
        }
        
        if (!dataContext.DataStore.ExistsInStore(hash))
            return NotFound;

        // Part of a workaround to prevent LBP Hub from breaking challenge ghost replay
        if (dataContext.Database.GetAssetFromHash(hash)?.AssetType == GameAssetType.ChallengeGhost)
        {
            if (ghostService.IsUserChallengeGhostRateLimited(user.UserId))
            {
                // Lie to the game by returning OK without any actual content, as any other responses (even successful ones)
                // will prompt LBP Hub to try and get every ungiven ghost asset a second time before giving up, 
                // which makes the game slower and might also screw with this rate limit.
                context.Logger.LogDebug(BunkumCategory.UserContent, $"OK with no content to deal with LBP Hub bugs");
                return OK;
            }
            else
            {
                // Continue with request normally, but also add user to rate limit for requests in the near future
                ghostService.AddUserToChallengeGhostRateLimit(user.UserId);
            }
        }

        if (!dataContext.DataStore.TryGetDataFromStore(hash, out byte[]? data))
            return InternalServerError;

        Debug.Assert(data != null);
        return new Response(data, ContentType.BinaryData);
    }

    [GameEndpoint("showNotUploaded", HttpMethods.Post, ContentType.Xml)]
    [GameEndpoint("filterResources", HttpMethods.Post, ContentType.Xml)]
    [MinimumRole(GameUserRole.Restricted)]
    [NullStatusCode(BadRequest)]
    public SerializedResourceList? GetAssetsMissingFromStore(RequestContext context, SerializedResourceList body, IDataStore dataStore)
    {
        if(body.Items.Any(hash => !CommonPatterns.Sha1Regex().IsMatch(hash)))
            return null;

        return new SerializedResourceList(body.Items.Where(r => !dataStore.ExistsInStore(context.IsPSP() ? $"psp/{r}" : r)));
    }
}