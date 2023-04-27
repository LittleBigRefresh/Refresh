using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer.Endpoints;
using JetBrains.Annotations;

namespace Refresh.GameServer.Endpoints;

[MeansImplicitUse]
public class ApiEndpointAttribute : EndpointAttribute
{
    // Lighthouse (referred to as legacy) is api v1
    public const string BaseRoute = "/api/v2/";

    public ApiEndpointAttribute(string route, Method method = Method.Get, ContentType contentType = ContentType.Json)
        : base(BaseRoute + route, method, contentType)
    {}

    public ApiEndpointAttribute(string route, ContentType contentType, Method method = Method.Get)
        : base(BaseRoute + route, contentType, method)
    {}
}