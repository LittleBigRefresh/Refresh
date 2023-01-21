using Refresh.GameServer.Configuration;
using Refresh.HttpServer;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer.Endpoints.Handshake;

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