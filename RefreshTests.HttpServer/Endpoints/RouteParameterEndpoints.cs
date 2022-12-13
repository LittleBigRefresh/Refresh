using Refresh.HttpServer;
using Refresh.HttpServer.Endpoints;

namespace RefreshTests.HttpServer.Endpoints;


public class RouteParameterEndpoints : EndpointGroup
{
    [Endpoint("/param/{input}")]
    public string Parameter(RequestContext context, string input)
    {
        return input;
    }
    
    [Endpoint("/params/{input}/{inputOther}")]
    public string Parameters(RequestContext context, string input, string inputOther)
    {
        return input + "," + inputOther;
    }
}