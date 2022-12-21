using System.Net;
using Refresh.HttpServer;
using RefreshTests.HttpServer.Endpoints;

namespace RefreshTests.HttpServer.Tests;

public class NullStatusCodeTests : ServerDependentTest
{
    [Test]
    public async Task ReturnsCorrectResponseWhenNull()
    {
        (RefreshHttpServer server, HttpClient client) = this.Setup();
        server.AddEndpointGroup<NullEndpoints>();

        HttpResponseMessage resp = await client.GetAsync("/null?null=true");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
    
    [Test]
    public async Task ReturnsCorrectResponseWhenNotNull()
    {
        (RefreshHttpServer server, HttpClient client) = this.Setup();
        server.AddEndpointGroup<NullEndpoints>();

        HttpResponseMessage resp = await client.GetAsync("/null?null=false");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}