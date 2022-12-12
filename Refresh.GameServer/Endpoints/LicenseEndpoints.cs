using System.Net;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer.Endpoints;

public class LicenseEndpoints : EndpointGroup
{
    [GameEndpoint("eula", Method.Get, ContentType.Plaintext)]
    public string License(HttpListenerContext context)
    {
        return "Welcome to Refresh!";
    }
}