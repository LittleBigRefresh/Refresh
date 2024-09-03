using Bunkum.Protocols.Http;
using JetBrains.Annotations;

namespace Refresh.PresenceServer.ApiServer;

[MeansImplicitUse]
public class ApiEndpointAttribute : HttpEndpointAttribute
{
    public const string BaseRoute = "/api/";
    
    public ApiEndpointAttribute(string route, HttpMethods method = HttpMethods.Get, string contentType = Bunkum.Listener.Protocol.ContentType.Plaintext) 
        : base(BaseRoute + route, method, contentType) 
    {}
    
    public ApiEndpointAttribute(string route, string contentType, HttpMethods method = HttpMethods.Get)
        : base(BaseRoute + route, method, contentType)
    {}
}