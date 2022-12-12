using System.Net;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace Refresh.TestServer.Endpoints;

public class TestEndpoint : Endpoint
{
    public override string Route => "/";
    public override Response GetResponse(HttpListenerContext context)
    {
        return new Response("test", ContentType.Plaintext);
    }
}