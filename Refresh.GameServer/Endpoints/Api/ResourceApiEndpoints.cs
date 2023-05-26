using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Bunkum.HttpServer.Storage;

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
}