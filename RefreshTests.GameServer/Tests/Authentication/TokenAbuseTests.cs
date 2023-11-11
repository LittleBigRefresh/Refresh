using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.UserData;

namespace RefreshTests.GameServer.Tests.Authentication;

public class TokenAbuseTests : GameServerTest
{
    [Test]
    public void CantUseGameTokenOnApi()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient gameClient = context.GetAuthenticatedClient(TokenType.Game, user);
        using HttpClient apiClient = context.GetAuthenticatedClient(TokenType.Api, user);
        
        HttpResponseMessage request = gameClient.GetAsync("/api/v3/users/me").Result;
        Assert.That(request.StatusCode, Is.EqualTo(Forbidden));
        
        request = apiClient.GetAsync("/api/v3/users/me").Result;
        Assert.That(request.StatusCode, Is.EqualTo(OK));
    }
    
    [Test]
    public void CantUseApiTokenOnGame()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient gameClient = context.GetAuthenticatedClient(TokenType.Game, user);
        using HttpClient apiClient = context.GetAuthenticatedClient(TokenType.Api, user);
        
        HttpResponseMessage request = apiClient.GetAsync("/lbp/eula").Result;
        Assert.That(request.StatusCode, Is.EqualTo(Forbidden));
        
        request = gameClient.GetAsync("/lbp/eula").Result;
        Assert.That(request.StatusCode, Is.EqualTo(OK));
    }
}