using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Bunkum.HttpServer.Storage;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer.Endpoints.Api;

public class ResourceApiEndpoints : EndpointGroup
{
    [ApiEndpoint("asset/{hash}")]
    [Authentication(false)]
    public Response DownloadGameAsset(RequestContext context, IDataStore dataStore, string hash)
    {
        if (string.IsNullOrWhiteSpace(hash)) return BadRequest;
        if (!dataStore.ExistsInStore(hash)) return NotFound;

        bool gotData = dataStore.TryGetDataFromStore(hash, out byte[]? data);
        if (data == null || !gotData) return InternalServerError;

        return new Response(data, ContentType.BinaryData);
    }
    
    [ApiEndpoint("asset/{hash}/info")]
    [Authentication(false)]
    public Response GetAssetInfo(RequestContext context, GameDatabaseContext database, string hash)
    {
        if (string.IsNullOrWhiteSpace(hash)) return BadRequest;

        GameAsset? asset = database.GetAssetFromHash(hash);
        if (asset == null) return NotFound;

        return new Response(asset, ContentType.Json);
    }
}