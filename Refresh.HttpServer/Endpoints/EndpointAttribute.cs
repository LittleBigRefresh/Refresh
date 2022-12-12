

using JetBrains.Annotations;
using Refresh.HttpServer.Responses;

namespace Refresh.HttpServer.Endpoints;

[MeansImplicitUse]
public class EndpointAttribute : Attribute
{
    public readonly string Route;
    public readonly Method Method;
    public readonly ContentType ContentType;

    public EndpointAttribute(string route, Method method = Method.Get, ContentType contentType = ContentType.Plaintext)
    {
        this.Route = route;
        this.Method = method;
        this.ContentType = contentType;
    }
}