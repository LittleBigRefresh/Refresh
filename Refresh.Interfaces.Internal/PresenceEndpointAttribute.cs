using Bunkum.Protocols.Http;
using JetBrains.Annotations;
using Refresh.Common.Constants;

namespace Refresh.Interfaces.Internal;

[MeansImplicitUse]
public class PresenceEndpointAttribute : HttpEndpointAttribute
{
    public const string BaseRoute = EndpointRoutes.PresenceBaseRoute;
    
    public PresenceEndpointAttribute(string route, HttpMethods method = HttpMethods.Get, string contentType = Bunkum.Listener.Protocol.ContentType.Plaintext) 
        : base(BaseRoute + route, method, contentType) 
    {}
    
    public PresenceEndpointAttribute(string route, string contentType, HttpMethods method = HttpMethods.Get)
        : base(BaseRoute + route, method, contentType)
    {}
}