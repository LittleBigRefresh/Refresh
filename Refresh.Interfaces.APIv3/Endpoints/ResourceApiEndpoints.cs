using AttribDoc.Attributes;
using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.RateLimit;
using Bunkum.Core.Responses;
using Bunkum.Core.Storage;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.Common.Verification;
using Refresh.Core.Authentication.Permission;
using Refresh.Core.Configuration;
using Refresh.Core.Importing;
using Refresh.Core.Services;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Data;

namespace Refresh.Interfaces.APIv3.Endpoints;

public class ResourceApiEndpoints : EndpointGroup
{
    private const int RequestTimeoutDuration = 60;
    private const int MaxRequestAmount = 50 * RequestTimeoutDuration; // requests per second
    private const int RequestBlockDuration = 30;
    private const string BucketName = "resource-api";
    
    [ApiV3Endpoint("assets/{hash}/download"), Authentication(false)]
    [ClientCacheResponse(31556952)] // 1 year, we don't expect the data to change
    [DocSummary("Downloads the raw data for an asset hash. Sent as application/octet-stream")]
    [DocError(typeof(ApiNotFoundError), "The asset could not be found")]
    [DocError(typeof(ApiInternalError), ApiInternalError.CouldNotGetAssetErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.HashMissingErrorWhen)]
    [RateLimitSettings(RequestTimeoutDuration, MaxRequestAmount, RequestBlockDuration, BucketName)]
    public Response DownloadGameAsset(RequestContext context, IDataStore dataStore,
        [DocSummary("The SHA1 hash of the asset")] string hash)
    {
        bool isPspAsset = hash.StartsWith("psp/");

        string realHash = isPspAsset ? hash[4..] : hash;

        if (!CommonPatterns.Sha1Regex().IsMatch(realHash)) return ApiValidationError.HashInvalidError;
        if (string.IsNullOrWhiteSpace(realHash)) return ApiValidationError.HashMissingError;
        if (!dataStore.ExistsInStore(hash)) return ApiNotFoundError.Instance;

        bool gotData = dataStore.TryGetDataFromStore(hash, out byte[]? data);
        if (data == null || !gotData) return ApiInternalError.CouldNotGetAssetError;

        return new Response(data, ContentType.BinaryData);
    }

    [ApiV3Endpoint("assets/psp/{hash}/download"), Authentication(false)]
    [ClientCacheResponse(31556952)] // 1 year, we don't expect the data to change
    [DocSummary("Downloads the raw data for a PSP asset hash. Sent as application/octet-stream")]
    [DocError(typeof(ApiNotFoundError), "The asset could not be found")]
    [DocError(typeof(ApiInternalError), ApiInternalError.CouldNotGetAssetErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.HashMissingErrorWhen)]
    [RateLimitSettings(RequestTimeoutDuration, MaxRequestAmount, RequestBlockDuration, BucketName)]
    public Response DownloadPspGameAsset(RequestContext context, IDataStore dataStore,
        [DocSummary("The SHA1 hash of the asset")] string hash)
        => this.DownloadGameAsset(context, dataStore, $"psp/{hash}");
    
    [ApiV3Endpoint("assets/{hash}/image", ContentType.Png), Authentication(false)]
    [ClientCacheResponse(9204111)] // 1 week, data may or may not change
    [DocSummary("Downloads any game texture (if it can be converted) as a PNG. Sent as image/png")]
    [DocError(typeof(ApiNotFoundError), "The asset could not be found")]
    [DocError(typeof(ApiInternalError), ApiInternalError.CouldNotGetAssetErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.HashMissingErrorWhen)]
    public Response DownloadGameAssetAsImage(RequestContext context, IDataStore dataStore, GameDatabaseContext database,
        [DocSummary("The SHA1 hash of the asset")] string hash, ImageImporter imageImport, AssetImporter assetImport)
    {
        bool isPspAsset = hash.StartsWith("psp/");

        string realHash = isPspAsset ? hash[4..] : hash;
        
        if (!CommonPatterns.Sha1Regex().IsMatch(realHash)) return ApiValidationError.HashInvalidError;
        if (string.IsNullOrWhiteSpace(realHash)) return ApiValidationError.HashMissingError;
        if (!dataStore.ExistsInStore(hash)) return ApiNotFoundError.Instance;

        if (!dataStore.ExistsInStore("png/" + realHash))
        {
            GameAssetType? convertedType = database.GetConvertedType(realHash);
            //If we found that the hash is a converted hash
            if (convertedType != null)
            {
                //Import the converted hash from the data store
                imageImport.ImportAsset(realHash, isPspAsset, convertedType.Value, dataStore);
            }
            //If not,
            else
            {
                //Import the asset as normal
                GameAsset? asset = database.GetAssetFromHash(realHash);
                imageImport.ImportAsset(realHash, isPspAsset, asset?.AssetType, dataStore);
            }
        }

        bool gotData = dataStore.TryGetDataFromStore("png/" + realHash, out byte[]? data);
        if (data == null || !gotData) return ApiInternalError.CouldNotGetAssetError;

        return new Response(data, ContentType.Png);
    }

    [ApiV3Endpoint("assets/psp/{hash}/image", ContentType.Png), Authentication(false)]
    [ClientCacheResponse(9204111)] // 1 week, data may or may not change
    [DocSummary("Downloads any PSP game texture (if it can be converted) as a PNG. Sent as image/png")]
    [DocError(typeof(ApiNotFoundError), "The asset could not be found")]
    [DocError(typeof(ApiInternalError), ApiInternalError.CouldNotGetAssetErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.HashMissingErrorWhen)]
    public Response DownloadPspGameAssetAsImage(RequestContext context, IDataStore dataStore, GameDatabaseContext database,
        [DocSummary("The SHA1 hash of the asset")] string hash, ImageImporter imageImport, AssetImporter assetImport) 
        => this.DownloadGameAssetAsImage(context, dataStore, database, $"psp/{hash}", imageImport, assetImport);

    [ApiV3Endpoint("assets/{hash}"), Authentication(false)]
    [DocSummary("Gets information from the database about a particular hash. Includes user who uploaded, dependencies, timestamps, etc.")]
    [DocError(typeof(ApiNotFoundError), "The asset could not be found")]
    [DocError(typeof(ApiValidationError), ApiValidationError.HashMissingErrorWhen)]
    public ApiResponse<ApiGameAssetResponse> GetAssetInfo(RequestContext context, GameDatabaseContext database,
        IDataStore dataStore,
        [DocSummary("The SHA1 hash of the asset")]
        string hash, DataContext dataContext)
    {
        bool isPspAsset = hash.StartsWith("psp/");

        string realHash = isPspAsset ? hash[4..] : hash;

        if (!CommonPatterns.Sha1Regex().IsMatch(realHash)) return ApiValidationError.HashInvalidError;
        if (string.IsNullOrWhiteSpace(realHash)) return ApiValidationError.HashMissingError;

        GameAsset? asset = database.GetAssetFromHash(realHash);
        if (asset == null) return ApiNotFoundError.Instance;

        return ApiGameAssetResponse.FromOld(asset, dataContext);
    }

    [ApiV3Endpoint("assets/psp/{hash}"), Authentication(false)]
    [DocSummary("Gets information from the database about a particular PSP hash. Includes user who uploaded, dependencies, timestamps, etc.")]
    [DocError(typeof(ApiValidationError), ApiValidationError.HashMissingErrorWhen)]
    [DocError(typeof(ApiNotFoundError), "The asset could not be found")]
    public ApiResponse<ApiGameAssetResponse> GetPspAssetInfo(RequestContext context, GameDatabaseContext database,
        IDataStore dataStore,
        [DocSummary("The SHA1 hash of the asset")]
        string hash, DataContext dataContext) => this.GetAssetInfo(context, database, dataStore, $"psp/{hash}", dataContext);

    [ApiV3Endpoint("assets/{hash}", HttpMethods.Post)]
    [RequireEmailVerified]
    [DocSummary("Uploads an image (PNG/JPEG) asset")]
    [DocError(typeof(ApiValidationError), ApiValidationError.HashMissingErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.BodyTooLongErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.CannotReadAssetErrorWhen)]
    [DocError(typeof(ApiValidationError), ApiValidationError.BodyMustBeImageErrorWhen)]
    [DocError(typeof(ApiAuthenticationError), ApiAuthenticationError.NoPermissionsForCreationWhen)]
    public ApiResponse<ApiGameAssetResponse> UploadImageAsset(RequestContext context, GameDatabaseContext database,
        IDataStore dataStore, AssetImporter importer, GameServerConfig config,
        [DocSummary("The SHA1 hash of the asset")]
        string hash,
        byte[] body, GameUser user, DataContext dataContext,
        AipiService? aipi,
        DiscordStaffService? discord,
        IntegrationConfig integration
        )
    {
        // If we're blocking asset uploads, throw unless the user is an admin.
        // We also have the ability to block asset uploads for trusted users (when they would normally bypass this)
        if (user.IsWriteBlocked(config)) 
            return ApiAuthenticationError.NoPermissionsForCreation;
        
        if (!CommonPatterns.Sha1Regex().IsMatch(hash)) return ApiValidationError.HashInvalidError;

        if (dataStore.ExistsInStore(hash))
        {
            GameAsset? existingAsset = database.GetAssetFromHash(hash);
            if (existingAsset == null)
                return ApiInternalError.HashNotFoundInDatabaseError;

            return ApiGameAssetResponse.FromOld(existingAsset, dataContext);
        }

        if (body.Length > 1_048_576 * 2)
        {
            return new ApiValidationError($"The asset must be under 2MB. Your file was {body.Length:N0} bytes.");
        }

        GameAsset? gameAsset = importer.ReadAndVerifyAsset(hash, body, TokenPlatform.Website, database);
        if (gameAsset == null)
            return ApiValidationError.CannotReadAssetError;

        if (gameAsset.AssetType is not GameAssetType.Jpeg and not GameAssetType.Png)
            return ApiValidationError.BodyMustBeImageError;

        if (!dataStore.WriteToStore(hash, body))
            return ApiInternalError.CouldNotWriteAssetError;
        
        gameAsset.OriginalUploader = user;
        
        if (aipi != null && aipi.ScanAndHandleAsset(dataContext, gameAsset))
        {
            return ApiModerationError.Instance;
        }
        
        database.AddAssetToDatabase(gameAsset);

        return new ApiResponse<ApiGameAssetResponse>(ApiGameAssetResponse.FromOld(gameAsset, dataContext)!, Created);
    }
}