using System.Net;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace RefreshTests.HttpServer;

public class TestEndpoint : Endpoint
{
    public const string TestString = "Test";
    
    public override string Route => "/";
    public override Response GetResponse(HttpListenerContext context)
    {
        return new Response(TestString, ContentType.Plaintext);
    }
}