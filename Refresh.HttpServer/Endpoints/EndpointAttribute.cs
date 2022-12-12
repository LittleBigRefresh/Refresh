

using JetBrains.Annotations;
using Refresh.HttpServer.Responses;

namespace Refresh.HttpServer.Endpoints;

[MeansImplicitUse]
public class EndpointAttribute : Attribute
{
    public string Route;
    public Method Method;
    public ContentType ContentType;

    public EndpointAttribute(string route, Method method, ContentType contentType)
    {
        this.Route = route;
        this.Method = method;
        this.ContentType = contentType;
    }
    
    public EndpointAttribute(string route, Method method)
    {
        this.Route = route;
        this.Method = method;
        this.ContentType = ContentType.Plaintext;
    }

    public EndpointAttribute(string route)
    {
        this.Route = route;
        this.Method = Method.Get;
        this.ContentType = ContentType.Plaintext;
    }
}