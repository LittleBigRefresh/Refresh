using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.UserData;

namespace RefreshTests.GameServer.Tests.Presence;

public class PresenceEndpointTests : GameServerTest
{
    [Test]
    public void GetTotalLevelCount()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        HttpClient http = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet2, TokenPlatform.PS3, user);
        
        Assert.That(http.GetAsync("/lbp/planetStats/highestSlotId").Result.Content.ReadAsStringAsync().Result, Is.EqualTo("0"));
        context.CreateLevel(user);
        Assert.That(http.GetAsync("/lbp/planetStats/highestSlotId").Result.Content.ReadAsStringAsync().Result, Is.EqualTo("1"));
        context.CreateLevel(user);
        Assert.That(http.GetAsync("/lbp/planetStats/highestSlotId").Result.Content.ReadAsStringAsync().Result, Is.EqualTo("2"));
    }
}