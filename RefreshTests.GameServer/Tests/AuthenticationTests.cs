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

        HttpClient authedClient = context.GetAuthenticatedClient(TokenType.Game, out string tokenData);
        
        Token? token = context.Database.GetTokenFromTokenData(tokenData, TokenType.Game);
        Assert.That(token, Is.Not.Null);
        Assert.That(token?.User, Is.Not.Null);

        HttpResponseMessage authedRequest = await authedClient.GetAsync("/lbp/playersInPodCount");
        Assert.That(authedRequest.StatusCode, Is.EqualTo(OK));
    }
}