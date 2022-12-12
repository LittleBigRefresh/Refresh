using JetBrains.Annotations;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer;

[MeansImplicitUse]
public class GameEndpointAttribute : EndpointAttribute
{
    private const string BaseRoute = "/LITTLEBIGPLANETPS3_XML/";
    
    public GameEndpointAttribute(string route, Method method, ContentType contentType) 
        : base(BaseRoute + route, method, contentType) 
    {}

    public GameEndpointAttribute(string route, Method method)
        : base(BaseRoute + route, method)
    {}

    public GameEndpointAttribute(string route)
        : base(BaseRoute + route)
    {}
}