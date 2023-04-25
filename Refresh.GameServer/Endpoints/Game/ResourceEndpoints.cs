using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Bunkum.HttpServer.Storage;
using Refresh.GameServer.Types.Lists;

namespace Refresh.GameServer.Endpoints.Game;

public class ResourceEndpoints : EndpointGroup
{
    //NOTE: type does nothing here, but it's sent by LBP so we have to accept it
    [GameEndpoint("upload/{hash}/{type}", Method.Post)]
    [GameEndpoint("upload/{hash}", Method.Post)]
    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    public Response UploadResource(RequestContext context, string hash, string type, byte[] body, IDataStore dataStore)
    {
        if (dataStore.ExistsInStore(hash))
            return HttpStatusCode.Conflict;

        if (!dataStore.WriteToStore(hash, body))
            return HttpStatusCode.InternalServerError;

        return HttpStatusCode.OK;
    }

    [GameEndpoint("r/{hash}")]
    public Response GetResource(RequestContext context, string hash, IDataStore dataStore)
    {
        if (!dataStore.ExistsInStore(hash))
            return HttpStatusCode.NotFound;

        if (!dataStore.TryGetDataFromStore(hash, out byte[]? data))
            return HttpStatusCode.InternalServerError;

        Debug.Assert(data != null);
        return new Response(data, ContentType.BinaryData);
    }

    [GameEndpoint("showNotUploaded", Method.Post, ContentType.Xml)]
    public ResourceList ShowNotUploaded(RequestContext context, ResourceList body, IDataStore dataStore) 
        => new(body.Items.Where(r => !dataStore.ExistsInStore(r)));
}