using System.Net;
using Refresh.HttpServer;
using Refresh.HttpServer.Authentication.Dummy;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace RefreshTests.HttpServer.Endpoints;

public class AuthenticationEndpoints : EndpointGroup
{
    [Endpoint("/auth", Method.Get, ContentType.Json)]
    [RequiresAuthentication]
    public DummyUser Authentication(RequestContext context, DummyUser user)
    {
        return user;
    }
}