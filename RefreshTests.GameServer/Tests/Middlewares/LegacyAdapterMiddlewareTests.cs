using System.Net.Http.Json;
using Refresh.GameServer.Middlewares;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;

namespace RefreshTests.GameServer.Tests.Middlewares;

public class LegacyAdapterMiddlewareTests : GameServerTest
{
    [Test]
    public void DoesntAdaptForOtherUrls()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddMiddleware<LegacyAdapterMiddleware>();
        context.Server.Value.Server.AddEndpointGroup<TestEndpoints>();

        ApiOkResponse? response = context.Http.GetFromJsonAsync<ApiOkResponse>("/api/v3/test").Result;
        
        Assert.That(response, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response!.Error, Is.Null);
            Assert.That(response.StatusCode, Is.EqualTo(OK));
        });
    }

    [Test]
    public void AdaptsForLegacyGameUrl()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddMiddleware<LegacyAdapterMiddleware>();
        context.Server.Value.Server.AddEndpointGroup<TestEndpoints>();

        HttpResponseMessage response = context.Http.GetAsync("/LITTLEBIGPLANETPS3_XML/test").Result;
        
        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(OK));
            Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("test"));
        });
    }
}