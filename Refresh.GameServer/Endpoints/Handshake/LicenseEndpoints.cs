using Refresh.HttpServer;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer.Endpoints.Handshake;

public class LicenseEndpoints : EndpointGroup
{
    [GameEndpoint("eula", Method.Get, ContentType.Plaintext)]
    public string License(RequestContext context)
    {
        //TODO: add configuration option for EULA
        return "Welcome to Refresh!";
    }

    [GameEndpoint("announce", Method.Get, ContentType.Plaintext)]
    public string Announce(RequestContext context) 
    {
        //TODO: add configuration option for announce
        return string.Empty;
    }
}