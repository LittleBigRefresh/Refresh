using System.Text;
using Refresh.GameServer.Middlewares;

namespace RefreshTests.GameServer.Tests.Middlewares;

public class DigestMiddlewareTests : GameServerTest
{
    [Test]
    public async Task DoesntIncludeDigestWhenOutsideOfGame()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddMiddleware<DigestMiddleware>();

        HttpResponseMessage response = await context.Http.GetAsync("/api/v3/instance");
        
        Assert.Multiple(() =>
        {
            Assert.That(response.Headers.Contains("X-Digest-A"), Is.False);
            Assert.That(response.Headers.Contains("X-Digest-B"), Is.False);
        });
    }
    
    [Test]
    public async Task IncludesDigestInGame()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddEndpointGroup<TestEndpoints>();
        context.Server.Value.Server.AddMiddleware<DigestMiddleware>();

        HttpResponseMessage response = await context.Http.GetAsync("/lbp/eula");
        
        Assert.Multiple(() =>
        {
            Assert.That(response.Headers.Contains("X-Digest-A"), Is.True);
            Assert.That(response.Headers.Contains("X-Digest-B"), Is.True);
        });
    }
    
    [Test]
    public async Task DigestIsCorrect()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddEndpointGroup<TestEndpoints>();
        context.Server.Value.Server.AddMiddleware<DigestMiddleware>();

        const string endpoint = "/lbp/test";
        const string expectedResultStr = "test";
        
        using MemoryStream blankMs = new();
        using MemoryStream expectedResultMs = new(Encoding.ASCII.GetBytes(expectedResultStr));
        
        string serverDigest = DigestMiddleware.CalculateDigest(endpoint, expectedResultMs, "", null, null);
        string clientDigest = DigestMiddleware.CalculateDigest(endpoint, blankMs, "", null, null);

        context.Http.DefaultRequestHeaders.Add("X-Digest-A", clientDigest);
        HttpResponseMessage response = await context.Http.GetAsync(endpoint);
        
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(OK));
            
            Assert.That(response.Headers.Contains("X-Digest-A"), Is.True);
            Assert.That(response.Headers.Contains("X-Digest-B"), Is.True);
            
            Assert.That(response.Headers.GetValues("X-Digest-A").First(), Is.EqualTo(serverDigest));
            Assert.That(response.Headers.GetValues("X-Digest-B").First(), Is.EqualTo(clientDigest));
        });
    }
    
    [Test]
    public async Task PspDigestIsCorrect()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddEndpointGroup<TestEndpoints>();
        context.Server.Value.Server.AddMiddleware<DigestMiddleware>();

        const string endpoint = "/lbp/test";
        const string expectedResultStr = "test";
        
        using MemoryStream blankMs = new();
        using MemoryStream expectedResultMs = new(Encoding.ASCII.GetBytes(expectedResultStr));
        
        string serverDigest = DigestMiddleware.CalculateDigest(endpoint, expectedResultMs, "", null, null);
        string clientDigest = DigestMiddleware.CalculateDigest(endpoint, blankMs, "", 205, 5);

        context.Http.DefaultRequestHeaders.Add("X-Digest-A", clientDigest);
        context.Http.DefaultRequestHeaders.Add("X-data-v", "5");
        context.Http.DefaultRequestHeaders.Add("X-exe-v", "205");
        HttpResponseMessage response = await context.Http.GetAsync(endpoint);
        
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(OK));
            
            Assert.That(response.Headers.Contains("X-Digest-A"), Is.True);
            Assert.That(response.Headers.Contains("X-Digest-B"), Is.True);
            
            Assert.That(response.Headers.GetValues("X-Digest-A").First(), Is.EqualTo(serverDigest));
            Assert.That(response.Headers.GetValues("X-Digest-B").First(), Is.EqualTo(clientDigest));
        });
    }

    [Test]
    public async Task FailsWhenDigestIsBad()
    {
        using TestContext context = this.GetServer();
        context.Server.Value.Server.AddEndpointGroup<TestEndpoints>();
        context.Server.Value.Server.AddMiddleware<DigestMiddleware>();
        
        context.Http.DefaultRequestHeaders.Add("X-Digest-A", "asdf");
        HttpResponseMessage response = await context.Http.GetAsync("/lbp/eula");
        
        Assert.Pass(); // TODO: we have no way of detecting a failed digest check
    }
}