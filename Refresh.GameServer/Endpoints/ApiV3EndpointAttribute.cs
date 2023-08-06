using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer.Endpoints;
using JetBrains.Annotations;

namespace Refresh.GameServer.Endpoints;

[MeansImplicitUse]
public class ApiV3EndpointAttribute : EndpointAttribute
{
    public const string BaseRoute = "/api/v3/";
    
    public string RouteWithParameters { get; }

    public ApiV3EndpointAttribute(string route, Method method = Method.Get, ContentType contentType = ContentType.Json)
        : base(BaseRoute + route, method, contentType)
    {
        this.RouteWithParameters = '/' + route;
    }

    public ApiV3EndpointAttribute(string route, ContentType contentType, Method method = Method.Get)
        : this(route, method, contentType)
    {}
}