using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Refresh.GameServer.Endpoints;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;

namespace RefreshTests.GameServer;

public class TestEndpoints : EndpointGroup
{
    [GameEndpoint("test"), Authentication(false)]
    public string TestGame(RequestContext context) => "test";

    [ApiV3Endpoint("test"), Authentication(false)]
    public ApiOkResponse TestApi(RequestContext context) => new();
}