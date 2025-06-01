using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Refresh.Interfaces.APIv3;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.Game;

namespace RefreshTests.GameServer;

public class TestEndpoints : EndpointGroup
{
    [GameEndpoint("test"), Authentication(false)]
    public string TestGame(RequestContext context) => "test";

    [ApiV3Endpoint("test"), Authentication(false)]
    public ApiOkResponse TestApi(RequestContext context) => new();
}