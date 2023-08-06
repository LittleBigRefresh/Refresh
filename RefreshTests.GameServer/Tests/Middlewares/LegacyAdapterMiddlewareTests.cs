using System.Net.Http.Json;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Middlewares;

namespace RefreshTests.GameServer.Tests.Middlewares;

public class LegacyAdapterMiddlewareTests : GameServerTest
{
    [Test]
    public async Task DoesntAdaptForOtherUrls()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddMiddleware<LegacyAdapterMiddleware>();
        context.Server.Value.Server.AddEndpointGroup<TestEndpoints>();

        ApiOkResponse? response = await context.Http.GetFromJsonAsync<ApiOkResponse>("/api/v3/test");
        
        Assert.That(response, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response!.Error, Is.Null);
            Assert.That(response.StatusCode, Is.EqualTo(OK));
        });
    }

    [Test]
    public async Task AdaptsForLegacyGameUrl()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddMiddleware<LegacyAdapterMiddleware>();
        context.Server.Value.Server.AddEndpointGroup<TestEndpoints>();
        
        HttpResponseMessage response = await context.Http.GetAsync("/LITTLEBIGPLANETPS3_XML/test");
        
        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(OK));
            Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("test"));
        });
    }
}