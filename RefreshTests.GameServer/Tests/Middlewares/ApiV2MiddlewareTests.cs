using Refresh.GameServer.Middlewares;

namespace RefreshTests.GameServer.Tests.Middlewares;

public class ApiV2MiddlewareTests : GameServerTest
{
    [Test]
    public async Task ErrorsWhenGoingToV2()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddMiddleware<ApiV2GoneMiddleware>();

        HttpResponseMessage response = await context.Http.GetAsync("/api/v2/asdf");
        
        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(Gone));
            Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("error"));
        });
    }
    
    [Test]
    public async Task OkWhenGoingAnywhereElse()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddMiddleware<ApiV2GoneMiddleware>();

        HttpResponseMessage response = await context.Http.GetAsync("/api/v3/instance");
        
        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(OK));
            Assert.That(await response.Content.ReadAsStringAsync(), Does.Not.Contain("error"));
        });
    }
}