using System.Net;
using Refresh.HttpServer;
using RefreshTests.HttpServer.Endpoints;

namespace RefreshTests.HttpServer.Tests;

public class ResponseTests : ServerDependentTest
{
    [Test]
    [TestCase("/response/string", HttpStatusCode.OK)]
    [TestCase("/response/responseObject", HttpStatusCode.OK)]
    [TestCase("/response/responseObjectWithCode", HttpStatusCode.Accepted)]
    public void CorrectResponseForAllTypes(string endpoint, HttpStatusCode codeToCheckFor)
    {
        (RefreshHttpServer? server, HttpClient? client) = this.Setup();
        server.AddEndpointGroup<ResponseEndpoints>();
        
        HttpResponseMessage msg = client.Send(new HttpRequestMessage(HttpMethod.Get, endpoint));
        Assert.Multiple(async () =>
        {
            Assert.That(await msg.Content.ReadAsStringAsync(), Is.EqualTo("works"));
            Assert.That(msg.StatusCode, Is.EqualTo(codeToCheckFor));
        });
    }
}