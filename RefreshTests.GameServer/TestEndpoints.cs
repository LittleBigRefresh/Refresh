using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Endpoints;

namespace RefreshTests.GameServer;

public class TestEndpoints : EndpointGroup
{
    [GameEndpoint("test"), Authentication(false)]
    public string TestGame(RequestContext context) => "test";
}