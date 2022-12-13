using JetBrains.Annotations;
using Refresh.HttpServer;
using Refresh.HttpServer.Endpoints;

namespace RefreshTests.HttpServer.Endpoints;

[NoReorder]
public class RouteStartsWithEndpoints : EndpointGroup
{
    [Endpoint("/sw/a")]
    public string A(RequestContext context)
    {
        return "a";
    }
    
    [Endpoint("/sw/asdf")]
    public string Asdf(RequestContext context)
    {
        return "asdf";
    }
}