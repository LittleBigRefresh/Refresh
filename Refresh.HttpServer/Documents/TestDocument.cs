using System.Net;
using Refresh.HttpServer.Responses;

namespace Refresh.HttpServer.Documents;

public class TestDocument : Document
{
    public override string Name => "Test Document";
    public override Response GetResponse(HttpListenerContext context)
    {
        return new Response("test", ContentType.Plaintext);
    }
}