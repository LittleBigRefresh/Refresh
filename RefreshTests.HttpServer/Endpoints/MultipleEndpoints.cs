using Refresh.HttpServer;
using Refresh.HttpServer.Endpoints;

namespace RefreshTests.HttpServer.Endpoints;

public class MultipleEndpoints : EndpointGroup
{
    [Endpoint("/a")]
    [Endpoint("/b")]
    public string Test(RequestContext context)
    {
        return "works";
    }
}