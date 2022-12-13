using Refresh.HttpServer;
using Refresh.HttpServer.Endpoints;

namespace RefreshTests.HttpServer.Endpoints;

public class TestEndpoints : EndpointGroup
{
    public const string TestString = "Test";

    [Endpoint("/")]
    public string Test(RequestContext context)
    {
        return TestString;
    }
}