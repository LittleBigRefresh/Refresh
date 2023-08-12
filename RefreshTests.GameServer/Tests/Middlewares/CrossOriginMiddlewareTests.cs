using Refresh.GameServer.Middlewares;

namespace RefreshTests.GameServer.Tests.Middlewares;

public class CrossOriginMiddlewareTests : GameServerTest
{
    [Test]
    public async Task PutsCorrectHeadersWhenGoingToV3()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddMiddleware<CrossOriginMiddleware>();

        HttpResponseMessage response = await context.Http.GetAsync("/api/v3/instance");
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(OK));
            Assert.That(response.Headers.Select(x => x.Key), Does.Contain("Access-Control-Allow-Origin"));
            Assert.That(response.Headers.Select(x => x.Key), Does.Not.Contain("Access-Control-Allow-Headers"));
        });
    }

    [Test]
    public async Task PutsCorrectHeadersWhenOptionsV3()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddMiddleware<CrossOriginMiddleware>();

        HttpResponseMessage response = await context.Http.SendAsync(new HttpRequestMessage(HttpMethod.Options, "/api/v3/instance"));
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(OK));
            Assert.That(response.Headers.Select(x => x.Key), Does.Contain("Access-Control-Allow-Origin"));
            Assert.That(response.Headers.Select(x => x.Key), Does.Contain("Access-Control-Allow-Headers"));
        });
    }
    
    [Test]
    public async Task PutsNoHeadersWhenAvoidingV3()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddMiddleware<CrossOriginMiddleware>();

        HttpResponseMessage response = await context.Http.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/lbp/eula"));
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(Forbidden));
            Assert.That(response.Headers.Select(x => x.Key), Does.Not.Contain("Access-Control-Allow-Origin"));
            Assert.That(response.Headers.Select(x => x.Key), Does.Not.Contain("Access-Control-Allow-Headers"));
        });
    }
}