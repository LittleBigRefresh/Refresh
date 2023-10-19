using Refresh.GameServer.Middlewares;

namespace RefreshTests.GameServer.Tests.Middlewares;

public class CrossOriginMiddlewareTests : GameServerTest
{
    [Test]
    public void PutsCorrectHeadersWhenGoingToV3()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddMiddleware<CrossOriginMiddleware>();

        HttpResponseMessage response = context.Http.GetAsync("/api/v3/instance").Result;
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(OK));
            Assert.That(response.Headers.Select(x => x.Key), Does.Contain("Access-Control-Allow-Origin"));
            Assert.That(response.Headers.Select(x => x.Key), Does.Not.Contain("Access-Control-Allow-Headers"));
        });
    }

    [Test]
    public void PutsCorrectHeadersWhenOptionsV3()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddMiddleware<CrossOriginMiddleware>();

        HttpResponseMessage response = context.Http.SendAsync(new HttpRequestMessage(HttpMethod.Options, "/api/v3/instance")).Result;
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(OK));
            Assert.That(response.Headers.Select(x => x.Key), Does.Contain("Access-Control-Allow-Origin"));
            Assert.That(response.Headers.Select(x => x.Key), Does.Contain("Access-Control-Allow-Headers"));
        });
    }
    
    [Test]
    public void PutsNoHeadersWhenAvoidingV3()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddMiddleware<CrossOriginMiddleware>();

        HttpResponseMessage response = context.Http.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/lbp/eula")).Result;
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(Forbidden));
            Assert.That(response.Headers.Select(x => x.Key), Does.Not.Contain("Access-Control-Allow-Origin"));
            Assert.That(response.Headers.Select(x => x.Key), Does.Not.Contain("Access-Control-Allow-Headers"));
        });
    }
}