using System.Net;
using System.Text;

namespace Refresh.HttpServer.Responses;

public struct Response
{
    public Response(byte[] data, ContentType contentType = ContentType.Html, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        this.StatusCode = statusCode;
        this.Data = data;
        this.ContentType = contentType;
    }

    public Response(object? data, ContentType contentType = ContentType.Html, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        this.ContentType = contentType;
        this.StatusCode = statusCode;
        this.Data = Encoding.Default.GetBytes(data?.ToString() ?? string.Empty);
    }

    public readonly HttpStatusCode StatusCode;
    public readonly ContentType ContentType;
    public readonly byte[] Data;
}