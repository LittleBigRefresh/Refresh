using MongoDB.Bson;
using NotEnoughLogs;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;
using RefreshTests.GameServer.Logging;

namespace RefreshTests.GameServer.Tests.Levels;

public class LevelListOverrideUnitTests
{
    [Test]
    public void CanOverrideLevel()
    {
        using Logger logger = new(new []{ new NUnitSink() });
        LevelListOverrideService service = new(logger);
        GameUser user = new()
        {
            UserId = new ObjectId("64ea5a8a7c412d18ab640fd1"),
            Username = "SuperDingus69",
        };

        GameLevel level = new()
        {
            LevelId = 1,
        };
        
        service.AddOverridesForUser(user, new []{level});
    }
}

public class LevelListOverrideIntegrationTests : GameServerTest
{
    [Test]
    public void CanGetOverriddenLevels()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user, "dingus 2B47430C-70F1-4A21-A1D0-EC3011A62239");

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game);
        
        // Verify that the endpoint isn't already attempting to return anything
        // This can be any endpoint that doesnt return all levels but I chose mmpicks
        HttpResponseMessage message = client.GetAsync("/lbp/slots/mmpicks").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        Assert.That(message.Content.ReadAsStringAsync().Result, Does.Not.Contain(level.Title));

        LevelListOverrideService overrideService = context.GetService<LevelListOverrideService>();
        Assert.That(overrideService.UserHasOverrides(user), Is.False);
    }
}