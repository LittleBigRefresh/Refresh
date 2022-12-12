using System.Net;
using Refresh.HttpServer.Endpoints;

namespace RefreshTests.HttpServer;

public class TestEndpoints : EndpointGroup
{
    public const string TestString = "Test";

    [Endpoint("/")]
    public string Test(HttpListenerContext context)
    {
        return TestString;
    }
}