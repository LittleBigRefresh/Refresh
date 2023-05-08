using Refresh.GameServer.Authentication;

namespace RefreshTests.GameServer.Tests;

public class AuthenticationTests : GameServerTest
{
    [Test]
    public async Task AuthenticationWorks()
    {
        TestContext context = this.GetServer();
        
        HttpResponseMessage unauthedRequest = await context.Http.GetAsync("/lbp/playersInPodCount");
        Assert.That(unauthedRequest.StatusCode, Is.EqualTo(Forbidden));

        HttpClient authedClient = context.GetAuthenticatedClient(TokenType.Game);
        Thread.Sleep(200);
        
        HttpResponseMessage authedRequest = await authedClient.GetAsync("/lbp/playersInPodCount");
        Assert.That(authedRequest.StatusCode, Is.EqualTo(OK));
    }
}