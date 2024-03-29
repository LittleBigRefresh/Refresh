using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;

namespace Refresh.GameServer.Endpoints;

public class LegacyApiEndpointAttribute : HttpEndpointAttribute
{
    public const string BaseRoute = "/api/v1/";

    public LegacyApiEndpointAttribute(string route, HttpMethods method = HttpMethods.Get, string contentType = Bunkum.Listener.Protocol.ContentType.Json)
        : base(BaseRoute + route, method, contentType)
    {}

    public LegacyApiEndpointAttribute(string route, string contentType, HttpMethods method = HttpMethods.Get)
        : base(BaseRoute + route, contentType, method)
    {}
}