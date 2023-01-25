using Bunkum.HttpServer.Endpoints;
using Bunkum.HttpServer.Responses;
using JetBrains.Annotations;

namespace Refresh.GameServer.Endpoints;

[MeansImplicitUse]
public class ApiEndpointAttribute : EndpointAttribute
{
    // v2, since maybe we want to add add v1 for backwards compatibility with project lighthouse?
    // LegacyApiEndpointAttribute for lighthouse api
    private const string BaseRoute = "/api/v2/";

    public ApiEndpointAttribute(string route, Method method = Method.Get, ContentType contentType = ContentType.Json)
        : base(BaseRoute + route, method, contentType)
    {}

    public ApiEndpointAttribute(string route, ContentType contentType, Method method = Method.Get)
        : base(BaseRoute + route, contentType, method)
    {}
}