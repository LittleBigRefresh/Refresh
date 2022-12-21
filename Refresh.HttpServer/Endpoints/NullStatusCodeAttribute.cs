using System.Net;

namespace Refresh.HttpServer.Endpoints;

[AttributeUsage(AttributeTargets.Method)]
public class NullStatusCodeAttribute : Attribute
{
    public readonly HttpStatusCode StatusCode;

    public NullStatusCodeAttribute(HttpStatusCode statusCode)
    {
        this.StatusCode = statusCode;
    }
}