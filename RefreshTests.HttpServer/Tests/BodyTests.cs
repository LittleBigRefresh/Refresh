using System.Net;
using Refresh.HttpServer;
using RefreshTests.HttpServer.Endpoints;

namespace RefreshTests.HttpServer.Tests;

public class BodyTests : ServerDependentTest
{
    [Test]
    [TestCase("/body/string")]
    [TestCase("/body/byteArray")]
    [TestCase("/body/stream")]
    public async Task CorrectResponseForAllTypes(string endpoint)
    {
        (RefreshHttpServer server, HttpClient client) = this.Setup();
        server.AddEndpointGroup<BodyEndpoints>();
        
        HttpResponseMessage msg = await client.PostAsync(endpoint, new StringContent("works"));
        Assert.That(await msg.Content.ReadAsStringAsync(), Is.EqualTo("works"));
    }

    [Test]
    public async Task ReturnsBadRequestOnNoData()
    {
        (RefreshHttpServer server, HttpClient client) = this.Setup();
        server.AddEndpointGroup<BodyEndpoints>();
        
        HttpResponseMessage msg = await client.PostAsync("/body/string", null);
        Assert.That(msg.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}