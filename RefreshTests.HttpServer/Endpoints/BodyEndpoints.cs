using System.Text;
using Refresh.HttpServer;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Responses;

namespace RefreshTests.HttpServer.Endpoints;

public class BodyEndpoints : EndpointGroup
{
    [Endpoint("/body/string", Method.Post)]
    public string String(RequestContext context, string body)
    {
        return body;
    }
    
    [Endpoint("/body/byteArray", Method.Post)]
    public string ByteArray(RequestContext context, byte[] body)
    {
        return Encoding.Default.GetString(body);
    }
    
    [Endpoint("/body/stream", Method.Post)]
    public string Stream(RequestContext context, Stream body)
    {
        MemoryStream stream = (MemoryStream)body;
        return Encoding.Default.GetString(stream.GetBuffer());
    }
}