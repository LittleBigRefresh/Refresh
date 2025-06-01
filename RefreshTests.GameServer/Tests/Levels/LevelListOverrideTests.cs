using MongoDB.Bson;
using NotEnoughLogs;
using Refresh.Core.Configuration;
using Refresh.Core.Services;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using RefreshTests.GameServer.Extensions;
using RefreshTests.GameServer.Logging;
using Refresh.Database.Models.Levels;
using Refresh.Interfaces.Game.Types.Lists;

namespace RefreshTests.GameServer.Tests.Levels;

public class LevelListOverrideUnitTests
{
    [Test]
    public void CanOverrideLevel()
    {
        using Logger logger = new([new NUnitSink()]);
        PlayNowService service = new(logger, new PresenceService(logger, new IntegrationConfig()));
        GameUser user = new()
        {
            UserId = new ObjectId("64ea5a8a7c412d18ab640fd1"),
            Username = "SuperDingus69",
        };

        GameLevel level = new()
        {
            LevelId = 1,
        };
        
        service.PlayNowLevel(user, level);
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

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        using HttpClient apiClient = context.GetAuthenticatedClient(TokenType.Api, user);
        
        // Verify that the endpoint isn't already attempting to return anything
        // This can be any endpoint that doesnt return all levels but I chose mmpicks
        HttpResponseMessage message = client.GetAsync("/lbp/slots/mmpicks").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        SerializedMinimalLevelList levelList = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(levelList.Items, Is.Empty);

        //Make sure we dont have an override set
        PlayNowService overrideService = context.GetService<PlayNowService>();
        Assert.That(overrideService.UserHasOverrides(user), Is.False);

        //Set a level as the override
        message = apiClient.PostAsync($"/api/v3/levels/id/{level.LevelId}/setAsOverride", new ByteArrayContent([])).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        context.Database.Refresh();
        
        //Assert that the override was set in the database properly
        Assert.That(overrideService.UserHasOverrides(user), Is.True);
        
        //Get the slots, and make sure it contains the level we set as the override
        message = client.GetAsync("/lbp/slots/mmpicks").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        levelList = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(levelList.Items, Has.Count.EqualTo(1));
        Assert.That(levelList.Items[0].LevelId, Is.EqualTo(level.LevelId));
        
        //Verify the team picks slot list has stopped pointing to the user override
        message = client.GetAsync("/lbp/slots/mmpicks").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        levelList = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(levelList.Items, Is.Empty);
    }
}