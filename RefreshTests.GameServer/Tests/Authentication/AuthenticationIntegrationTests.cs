using Refresh.GameServer.Authentication;

namespace RefreshTests.GameServer.Tests.Authentication;

public class AuthenticationIntegrationTests : GameServerTest
{
    [Test]
    public async Task GameAuthenticationWorks()
    {
        using TestContext context = this.GetServer();
        
        HttpResponseMessage unauthedRequest = await context.Http.GetAsync("/lbp/eula");
        Assert.That(unauthedRequest.StatusCode, Is.EqualTo(Forbidden));

        HttpClient authedClient = context.GetAuthenticatedClient(TokenType.Game, out string tokenData);
        
        Token? token = context.Database.GetTokenFromTokenData(tokenData, TokenType.Game);
        Assert.That(token, Is.Not.Null);
        Assert.That(token?.User, Is.Not.Null);

        HttpResponseMessage authedRequest = await authedClient.GetAsync("/lbp/eula");
        Assert.That(authedRequest.StatusCode, Is.EqualTo(OK));
    }
    
    [Test]
    public async Task ApiAuthenticationWorks()
    {
        using TestContext context = this.GetServer();
        
        HttpResponseMessage unauthedRequest = await context.Http.GetAsync("/api/v3/users/me");
        Assert.That(unauthedRequest.StatusCode, Is.EqualTo(Forbidden));

        HttpClient authedClient = context.GetAuthenticatedClient(TokenType.Api, out string tokenData);
        
        Token? token = context.Database.GetTokenFromTokenData(tokenData, TokenType.Api);
        Assert.That(token, Is.Not.Null);
        Assert.That(token?.User, Is.Not.Null);

        // TODO: Fix serialization of ObjectId
        HttpResponseMessage response = await authedClient.GetAsync("/api/v3/users/me");
        // (GameUser? user, HttpResponseMessage response) = await authedClient.GetJsonObjectAsync<GameUser>("/api/v3/user/me");
        Assert.Multiple(async () =>
        {
            // Assert.That(user, Is.Not.Null);
            Assert.That(await response.Content.ReadAsStringAsync(), Contains.Substring(token!.User.UserId.ToString()));
            Assert.That(response.StatusCode, Is.EqualTo(OK));
        });
    }
}