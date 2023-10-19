using Refresh.GameServer.Middlewares;

namespace RefreshTests.GameServer.Tests.Middlewares;

public class ApiV2MiddlewareTests : GameServerTest
{
    [Test]
    public void ErrorsWhenGoingToV2()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddMiddleware<ApiV2GoneMiddleware>();

        HttpResponseMessage response = context.Http.GetAsync("/api/v2/asdf").WaitResult();
        
        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(Gone));
            Assert.That(response.Content.ReadAsStringAsync().WaitResult(), Does.Contain("error"));
        });
    }
    
    [Test]
    public void OkWhenGoingAnywhereElse()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddMiddleware<ApiV2GoneMiddleware>();

        HttpResponseMessage response = context.Http.GetAsync("/api/v3/instance").WaitResult();
        
        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(OK));
            Assert.That(response.Content.ReadAsStringAsync().WaitResult(), Does.Not.Contain("error"));
        });
    }
}