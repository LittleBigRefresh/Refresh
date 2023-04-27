using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer.Endpoints;

namespace Refresh.GameServer.Endpoints;

public class LegacyApiEndpointAttribute : EndpointAttribute
{
    public const string BaseRoute = "/api/v1/";

    public LegacyApiEndpointAttribute(string route, Method method = Method.Get, ContentType contentType = ContentType.Json)
        : base(BaseRoute + route, method, contentType)
    {}

    public LegacyApiEndpointAttribute(string route, ContentType contentType, Method method = Method.Get)
        : base(BaseRoute + route, contentType, method)
    {}
}