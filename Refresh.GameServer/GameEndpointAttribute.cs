using JetBrains.Annotations;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace Refresh.GameServer;

[MeansImplicitUse]
public class GameEndpointAttribute : EndpointAttribute
{
    private const string BaseRoute = "/LITTLEBIGPLANETPS3_XML/";
    
    public GameEndpointAttribute(string route, Method method = Method.Get, ContentType contentType = ContentType.Plaintext) 
        : base(BaseRoute + route, method, contentType) 
    {}
    
    public GameEndpointAttribute(string route, ContentType contentType, Method method = Method.Get)
        : base(BaseRoute + route, method, contentType)
    {}
}