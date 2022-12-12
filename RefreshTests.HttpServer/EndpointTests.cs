using System.Net;
using System.Net.NetworkInformation;
using JetBrains.Annotations;
using Refresh.HttpServer;

namespace RefreshTests.HttpServer;

public class Tests
{
    private int _lowestPort = 32768;
    
    [Pure]
    private (RefreshHttpServer, HttpClient) Setup()
    {
        // find free port to use for listener
        // we should probably find a way to send requests directly to the HttpListener, but this works for now
        // adapted from https://stackoverflow.com/a/53936819
        IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
        IPEndPoint[] listeners = properties.GetActiveTcpListeners();
        
        foreach (int port in Enumerable.Range(this._lowestPort, 65536))
        {
            int? openPort = listeners.Select(item => item.Port).FirstOrDefault(p => p != port);
            
            if (openPort != null) continue;
            this._lowestPort = openPort!.Value;
            break;
        }

        Uri uri = new($"http://127.0.0.1:{this._lowestPort}/");
        this._lowestPort++;

        RefreshHttpServer server = new(uri);
        server.Start();

        HttpClient client = new();
        client.BaseAddress = uri;

        return (server, client);
    }

    [Test]
    public void ReturnsEndpoint()
    {
        (RefreshHttpServer? server, HttpClient? client) = this.Setup();
        
        server.AddEndpoint<TestEndpoint>();
        HttpResponseMessage msg = client.Send(new HttpRequestMessage(HttpMethod.Get, "/"));
        
        Assert.Multiple(async () =>
        {
            Assert.That(msg.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(await msg.Content.ReadAsStringAsync(), Is.EqualTo(TestEndpoint.TestString));
        });
    }

    [Test]
    public void ReturnsNotFound()
    {
        (RefreshHttpServer? _, HttpClient? client) = this.Setup();
        
        HttpResponseMessage msg = client.Send(new HttpRequestMessage(HttpMethod.Get, "/"));
        Assert.Multiple(async () =>
        {
            Assert.That(await msg.Content.ReadAsStringAsync(), Is.EqualTo("Not found: /"));
            Assert.That(msg.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }
}