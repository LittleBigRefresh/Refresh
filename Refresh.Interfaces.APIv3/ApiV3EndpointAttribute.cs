using Bunkum.Protocols.Http;
using JetBrains.Annotations;

namespace Refresh.Interfaces.APIv3;

[MeansImplicitUse]
public class ApiV3EndpointAttribute : HttpEndpointAttribute
{
    public const string BaseRoute = "/api/v3/";
    
    public string RouteWithParameters { get; }

    public ApiV3EndpointAttribute(string route, HttpMethods method = HttpMethods.Get, string contentType = Bunkum.Listener.Protocol.ContentType.Json)
        : base(BaseRoute + route, method, contentType)
    {
        this.RouteWithParameters = '/' + route;
    }

    public ApiV3EndpointAttribute(string route, string contentType, HttpMethods method = HttpMethods.Get)
        : this(route, method, contentType)
    {}
}