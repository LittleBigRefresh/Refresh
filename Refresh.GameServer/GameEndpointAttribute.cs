using JetBrains.Annotations;
using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;

namespace Refresh.GameServer;

[MeansImplicitUse]
public class GameEndpointAttribute : EndpointAttribute
{
    private const string BaseRoute = "/lbp/";
    
    public GameEndpointAttribute(string route, Method method = Method.Get, ContentType contentType = ContentType.Plaintext) 
        : base(BaseRoute + route, method, contentType) 
    {}
    
    public GameEndpointAttribute(string route, ContentType contentType, Method method = Method.Get)
        : base(BaseRoute + route, method, contentType)
    {}
}