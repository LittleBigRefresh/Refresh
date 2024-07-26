using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Core.Storage;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Importing;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types.Assets;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Verification;

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
        // If we're blocking asset uploads, throw unless the user is an admin.
        // We also have the ability to block asset uploads for trusted users (when they would normally bypass this)
        if (config.BlockAssetUploads && user.Role != GameUserRole.Admin)
        {
            if (user.Role < GameUserRole.Trusted || config.BlockAssetUploadsForTrustedUsers)
            {
                return Unauthorized;
            }
        }
        
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

        AssetSafetyLevel safetyLevel = config.MaximumAssetSafetyLevel;
        if (user.Role >= GameUserRole.Trusted)
            safetyLevel = config.MaximumAssetSafetyLevelForTrustedUsers;
       
        // Dont block any assets uploaded from PSP, and block any unwanted assets,
        // for example, if asset safety level is Dangerous (2) and maximum is configured as Safe (0), return 401
        // if asset safety is Safe (0), and maximum is configured as Safe (0), proceed
        if (gameAsset.SafetyLevel > safetyLevel && !isPSP)
        {
            context.Logger.LogWarning(BunkumCategory.UserContent, $"{gameAsset.AssetType} {hash} by {user} is above configured safety limit " +
                                                                 $"({gameAsset.SafetyLevel} > {safetyLevel})");
            return Unauthorized;
        }

        if (isPSP && gameAsset.SafetyLevel == AssetSafetyLevel.SafeMedia && safetyLevel < AssetSafetyLevel.SafeMedia)
        {
            context.Logger.LogWarning(BunkumCategory.UserContent, $"{gameAsset.AssetType} {hash} by {user} cannot be uploaded because media is disabled");
            return Unauthorized;
        }

        if (!dataStore.WriteToStore(assetPath, body))
            return InternalServerError;

        gameAsset.OriginalUploader = user;
        database.AddAssetToDatabase(gameAsset);
        
        database.IncrementUserFilesizeQuota(user, body.Length);
        
        context.Logger.LogInfo(BunkumCategory.UserContent, $"{user} uploaded a {gameAsset.AssetType} ({body.Length / 1024f:F1} KB)");
        return OK;
    }

    [GameEndpoint("r/{hash}")]
    [MinimumRole(GameUserRole.Restricted)]
    public Response GetResource(RequestContext context, string hash, IDataStore dataStore, GameDatabaseContext database, Token token)
    {
        if (!CommonPatterns.Sha1Regex().IsMatch(hash)) return BadRequest;
        
        //If the request comes from a PSP client,
        if (context.IsPSP())
        {
            //Point the hash into the `psp` folder
            hash = $"psp/{hash}";
        }
        
        if (!dataStore.ExistsInStore(hash))
            return NotFound;

        if (!dataStore.TryGetDataFromStore(hash, out byte[]? data))
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