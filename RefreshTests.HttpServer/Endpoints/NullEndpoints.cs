using System.Net;
using Refresh.HttpServer;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace RefreshTests.HttpServer.Endpoints;

public class NullEndpoints : EndpointGroup
{
    [Endpoint("/null", ContentType.Json)]
    [NullStatusCode(HttpStatusCode.BadRequest)]
    public object? Null(RequestContext context)
    {
        if (context.Request.QueryString["null"] == "true") return null;
        return new object();
    }
}