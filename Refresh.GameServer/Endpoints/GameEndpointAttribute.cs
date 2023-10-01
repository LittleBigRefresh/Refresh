using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using JetBrains.Annotations;

namespace Refresh.GameServer.Endpoints;

[MeansImplicitUse]
public class GameEndpointAttribute : HttpEndpointAttribute
{
    public const string BaseRoute = "/lbp/";
    
    public GameEndpointAttribute(string route, HttpMethods method = HttpMethods.Get, ContentType contentType = ContentType.Plaintext) 
        : base(BaseRoute + route, method, contentType) 
    {}
    
    public GameEndpointAttribute(string route, ContentType contentType, HttpMethods method = HttpMethods.Get)
        : base(BaseRoute + route, method, contentType)
    {}
}