using Bunkum.Protocols.Http;
using JetBrains.Annotations;

namespace Refresh.Interfaces.Game;

[MeansImplicitUse]
public class GameEndpointAttribute : HttpEndpointAttribute
{
    public const string BaseRoute = "/lbp/";
    
    public GameEndpointAttribute(string route, HttpMethods method = HttpMethods.Get, string contentType = Bunkum.Listener.Protocol.ContentType.Plaintext) 
        : base(BaseRoute + route, method, contentType) 
    {}
    
    public GameEndpointAttribute(string route, string contentType, HttpMethods method = HttpMethods.Get)
        : base(BaseRoute + route, method, contentType)
    {}
}