using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using Refresh.GameServer.Types.Lists;

namespace Refresh.GameServer.Endpoints.Game;

public class ResourceEndpoints : EndpointGroup
{
    [GameEndpoint("upload/{hash}", Method.Post)]
    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    public Response UploadResource(RequestContext context, string hash, byte[] body)
    {
        if (context.DataStore.ExistsInStore(hash))
            return new Response(HttpStatusCode.Conflict);

        if (!context.DataStore.WriteToStore(hash, body))
            return new Response(HttpStatusCode.InternalServerError);

        return new Response(HttpStatusCode.OK);
    }

    [GameEndpoint("r/{hash}")]
    public Response GetResource(RequestContext context, string hash)
    {
        if (!context.DataStore.ExistsInStore(hash))
            return new Response(HttpStatusCode.NotFound);

        if (!context.DataStore.TryGetDataFromStore(hash, out byte[]? data))
            return new Response(HttpStatusCode.InternalServerError);

        Debug.Assert(data != null);
        return new Response(data, ContentType.BinaryData);
    }

    [GameEndpoint("showNotUploaded", Method.Post, ContentType.Xml)]
    public ResourceList ShowNotUploaded(RequestContext context, ResourceList body) 
        => new(body.Items.Where(r => !context.DataStore.ExistsInStore(r)));
}