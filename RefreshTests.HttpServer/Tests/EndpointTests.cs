using System.Net;
using Refresh.HttpServer;
using RefreshTests.HttpServer.Endpoints;

namespace RefreshTests.HttpServer.Tests;

public class EndpointTests : ServerDependentTest
{
    [Test]
    public void ReturnsEndpoint()
    {
        (RefreshHttpServer server, HttpClient client) = this.Setup();
        
        HttpResponseMessage msg = client.Send(new HttpRequestMessage(HttpMethod.Get, "/"));
        Assert.That(msg.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        
        server.AddEndpointGroup<TestEndpoints>();
        
        msg = client.Send(new HttpRequestMessage(HttpMethod.Get, "/"));
        Assert.Multiple(async () =>
        {
            Assert.That(msg.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(await msg.Content.ReadAsStringAsync(), Is.EqualTo(TestEndpoints.TestString));
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
    
    [Test]
    public void MultipleEndpointAttributesWork()
    {
        (RefreshHttpServer server, HttpClient client) = this.Setup();
        server.AddEndpointGroup<MultipleEndpoints>();
        
        HttpResponseMessage msg = client.Send(new HttpRequestMessage(HttpMethod.Get, "/a"));
        Assert.Multiple(async () =>
        {
            Assert.That(msg.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(await msg.Content.ReadAsStringAsync(), Is.EqualTo("works"));
        });
        
        msg = client.Send(new HttpRequestMessage(HttpMethod.Get, "/b"));
        Assert.Multiple(async () =>
        {
            Assert.That(msg.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(await msg.Content.ReadAsStringAsync(), Is.EqualTo("works"));
        });
    }
}