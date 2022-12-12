using System.Net;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer.Endpoints;

public class AuthenticationEndpoints : EndpointGroup
{
    [GameEndpoint("login", Method.Post, ContentType.Xml)]
    public string Authenticate(HttpListenerContext context)
    {
        return "<loginResult><authTicket>yeah</authTicket><lbpEnvVer>Refresh</lbpEnvVer></loginResult>";
    }
}