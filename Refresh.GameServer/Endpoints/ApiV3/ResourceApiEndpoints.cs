using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Bunkum.HttpServer.Storage;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Errors;
using Refresh.GameServer.Importing;
using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer.Endpoints.ApiV3;

public class ResourceApiEndpoints
{
    private static readonly ApiValidationError HashMissingError = new("The hash is missing or null.");
    private static readonly Response HashMissingErrorResponse = HashMissingError;

    private static readonly ApiInternalError CouldNotGetAssetError = new("An error occurred while retrieving the asset from the data store.");
    private static readonly Response CouldNotGetAssetErrorResponse = CouldNotGetAssetError;
    
    [ApiV3Endpoint("asset/{hash}/download"), Authentication(false)]
    [ClientCacheResponse(31556952)] // 1 year, we don't expect the data to change
    public Response DownloadGameAsset(RequestContext context, IDataStore dataStore, string hash)
    {
        if (string.IsNullOrWhiteSpace(hash)) return HashMissingErrorResponse;
        if (!dataStore.ExistsInStore(hash)) return ApiNotFoundError.Instance;

        bool gotData = dataStore.TryGetDataFromStore(hash, out byte[]? data);
        if (data == null || !gotData) return CouldNotGetAssetErrorResponse;

        return new Response(data, ContentType.BinaryData);
    }
    
    [ApiV3Endpoint("asset/{hash}/image", ContentType.Png), Authentication(false)]
    [Endpoint("/gameAssets/{hash}", ContentType.Png)]
    [ClientCacheResponse(9204111)] // 1 week, data may or may not change
    public Response DownloadGameAssetAsImage(RequestContext context, IDataStore dataStore, string hash, GameDatabaseContext database)
    {
        if (string.IsNullOrWhiteSpace(hash)) return HashMissingErrorResponse;
        if (!dataStore.ExistsInStore(hash)) return ApiNotFoundError.Instance;

        if (!dataStore.ExistsInStore("png/" + hash))
        {
            GameAsset? asset = database.GetAssetFromHash(hash);
            if (asset == null) return ApiNotFoundError.Instance;
            
            ImageImporter.ImportAsset(asset, dataStore);
        }

        bool gotData = dataStore.TryGetDataFromStore("png/" + hash, out byte[]? data);
        if (data == null || !gotData) return CouldNotGetAssetErrorResponse;

        return new Response(data, ContentType.Png);
    }

    
    [ApiV3Endpoint("asset/{hash}"), Authentication(false)]
    public ApiResponse<GameAsset> GetAssetInfo(RequestContext context, GameDatabaseContext database, string hash)
    {
        if (string.IsNullOrWhiteSpace(hash)) return HashMissingError;

        GameAsset? asset = database.GetAssetFromHash(hash);
        if (asset == null) return ApiNotFoundError.Instance;

        return asset;
    }
}