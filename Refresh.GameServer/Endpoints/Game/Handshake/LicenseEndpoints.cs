using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Configuration;

namespace Refresh.GameServer.Endpoints.Game.Handshake;

public class LicenseEndpoints : EndpointGroup
{
    [GameEndpoint("eula")]
    public string License(RequestContext context, GameServerConfig config)
    {
        return config.LicenseText;
    }

    [GameEndpoint("announce")]
    public string Announce(RequestContext context, GameServerConfig config) 
    {
        return config.AnnounceText;
    }
}