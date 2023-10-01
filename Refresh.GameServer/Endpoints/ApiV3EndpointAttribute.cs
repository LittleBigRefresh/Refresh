using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using JetBrains.Annotations;

namespace Refresh.GameServer.Endpoints;

[MeansImplicitUse]
public class ApiV3EndpointAttribute : HttpEndpointAttribute
{
    public const string BaseRoute = "/api/v3/";
    
    public string RouteWithParameters { get; }

    public ApiV3EndpointAttribute(string route, HttpMethods method = HttpMethods.Get, ContentType contentType = ContentType.Json)
        : base(BaseRoute + route, method, contentType)
    {
        this.RouteWithParameters = '/' + route;
    }

    public ApiV3EndpointAttribute(string route, ContentType contentType, HttpMethods method = HttpMethods.Get)
        : this(route, method, contentType)
    {}
}