using System.Net;
using Refresh.HttpServer;
using RefreshTests.HttpServer.Authentication;
using RefreshTests.HttpServer.Endpoints;

namespace RefreshTests.HttpServer.Tests;

public class AuthenticationTests : ServerDependentTest
{
    [Test]
    public async Task WorksWhenAuthenticated()
    {
        (RefreshHttpServer server, HttpClient client) = this.Setup();
        server.AddEndpointGroup<AuthenticationEndpoints>();
        
        HttpResponseMessage msg = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth"));
        
        Assert.Multiple(async () =>
        {
            Assert.That(msg.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(await msg.Content.ReadAsStringAsync(), Is.EqualTo("{\"userId\":1,\"username\":\"Dummy\"}"));
        });
    }
    
    [Test]
    public async Task FailsWhenNotAuthenticated()
    {
        (RefreshHttpServer server, HttpClient client) = this.Setup();
        server.AddEndpointGroup<AuthenticationEndpoints>();
        
        client.DefaultRequestHeaders.Add("dummy-skip-auth", "true");
        HttpResponseMessage msg = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth"));
        
        Assert.That(msg.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    [Timeout(2000)] // ms
    public async Task CanSwitchAuthenticationProviders()
    {
        (RefreshHttpServer? server, HttpClient? client) = this.Setup(false);
        // Pass test when authentication provider is called
        server.UseAuthenticationProvider(new CallbackAuthenticationProvider(Assert.Pass));
        server.AddEndpointGroup<AuthenticationEndpoints>();
        server.Start();
        
        await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth"));
    }
}