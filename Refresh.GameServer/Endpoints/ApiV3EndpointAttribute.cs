using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer.Endpoints;
using JetBrains.Annotations;

namespace Refresh.GameServer.Endpoints;

[MeansImplicitUse]
public class ApiV3EndpointAttribute : EndpointAttribute
{
    public const string BaseRoute = "/api/v3/";

    public ApiV3EndpointAttribute(string route, Method method = Method.Get, ContentType contentType = ContentType.Json)
        : base(BaseRoute + route, method, contentType)
    {}

    public ApiV3EndpointAttribute(string route, ContentType contentType, Method method = Method.Get)
        : base(BaseRoute + route, contentType, method)
    {}
}