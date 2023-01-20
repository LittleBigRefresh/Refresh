using Refresh.GameServer.Configuration;
using Refresh.HttpServer;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer.Endpoints.Handshake;

public class LicenseEndpoints : EndpointGroup
{
    [GameEndpoint("eula")]
    public string License(RequestContext context)
    {
        //TODO: add configuration option for EULA
        return "Welcome to Refresh!";
    }

    [GameEndpoint("announce")]
    public string Announce(RequestContext context, IGameServerConfig config) 
    {
        return config.AnnounceText;
    }
}