using System.Net;
using Refresh.HttpServer.Authentication.Dummy;
using Refresh.HttpServer.Endpoints;

namespace RefreshTests.HttpServer.Endpoints;

public class AuthenticationEndpoints : EndpointGroup
{
    [Endpoint("/auth")]
    [RequiresAuthentication]
    public string Authentication(HttpListenerContext context, DummyUser user)
    {
        return "works";
    }
}