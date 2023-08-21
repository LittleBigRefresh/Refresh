using AttribDoc.Attributes;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Bunkum.HttpServer.Storage;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Importing;
using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class ResourceApiEndpoints : EndpointGroup
{
    private const string HashMissingErrorWhen = "The hash is missing or null";
    private static readonly ApiValidationError HashMissingError = new(HashMissingErrorWhen);
    private static readonly Response HashMissingErrorResponse = HashMissingError;

    private const string CouldNotGetAssetErrorWhen = "An error occurred while retrieving the asset from the data store";
    private static readonly ApiInternalError CouldNotGetAssetError = new(CouldNotGetAssetErrorWhen);
    private static readonly Response CouldNotGetAssetErrorResponse = CouldNotGetAssetError;
    
    [ApiV3Endpoint("assets/{hash}/download"), Authentication(false)]
    [ClientCacheResponse(31556952)] // 1 year, we don't expect the data to change
    [DocSummary("Downloads the raw data for an asset hash. Sent as application/octet-stream")]
    [DocError(typeof(ApiNotFoundError), "The asset could not be found")]
    [DocError(typeof(ApiInternalError), CouldNotGetAssetErrorWhen)]
    [DocError(typeof(ApiValidationError), HashMissingErrorWhen)]
    public Response DownloadGameAsset(RequestContext context, IDataStore dataStore,
        [DocSummary("The SHA1 hash of the asset")] string hash)
    {
        if (string.IsNullOrWhiteSpace(hash)) return HashMissingErrorResponse;
        if (!dataStore.ExistsInStore(hash)) return ApiNotFoundError.Instance;

        bool gotData = dataStore.TryGetDataFromStore(hash, out byte[]? data);
        if (data == null || !gotData) return CouldNotGetAssetErrorResponse;

        return new Response(data, ContentType.BinaryData);
    }
    
    [ApiV3Endpoint("assets/{hash}/image", ContentType.Png), Authentication(false)]
    [ClientCacheResponse(9204111)] // 1 week, data may or may not change
    [DocSummary("Downloads any game texture (if it can be converted) as a PNG. Sent as image/png")]
    [DocError(typeof(ApiNotFoundError), "The asset could not be found")]
    [DocError(typeof(ApiInternalError), CouldNotGetAssetErrorWhen)]
    [DocError(typeof(ApiValidationError), HashMissingErrorWhen)]
    public Response DownloadGameAssetAsImage(RequestContext context, IDataStore dataStore, GameDatabaseContext database,
        [DocSummary("The SHA1 hash of the asset")] string hash)
    {
        if (string.IsNullOrWhiteSpace(hash)) return HashMissingErrorResponse;
        if (!dataStore.ExistsInStore(hash)) return ApiNotFoundError.Instance;

        if (!dataStore.ExistsInStore("png/" + hash))
        {
            GameAsset? asset = database.GetAssetFromHash(hash);
            if (asset == null) return CouldNotGetAssetErrorResponse;
            
            ImageImporter.ImportAsset(asset, dataStore);
        }

        bool gotData = dataStore.TryGetDataFromStore("png/" + hash, out byte[]? data);
        if (data == null || !gotData) return CouldNotGetAssetErrorResponse;

        return new Response(data, ContentType.Png);
    }

    
    [ApiV3Endpoint("assets/{hash}"), Authentication(false)]
    [DocSummary("Gets information from the database about a particular hash. Includes user who uploaded, dependencies, timestamps, etc.")]
    [DocError(typeof(ApiValidationError), HashMissingErrorWhen)]
    [DocError(typeof(ApiNotFoundError), "The asset could not be found")]
    public ApiResponse<ApiGameAssetResponse> GetAssetInfo(RequestContext context, GameDatabaseContext database,
        [DocSummary("The SHA1 hash of the asset")] string hash)
    {
        if (string.IsNullOrWhiteSpace(hash)) return HashMissingError;

        GameAsset? asset = database.GetAssetFromHash(hash);
        if (asset == null) return ApiNotFoundError.Instance;

        return ApiGameAssetResponse.FromOld(asset);
    }
}