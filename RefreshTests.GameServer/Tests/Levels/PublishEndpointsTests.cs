using Refresh.Common.Constants;
using Refresh.Core.Configuration;
using Refresh.Database.Query;
using Refresh.Database.Models.Authentication;
using Refresh.Database;
using Refresh.Database.Models;
using Refresh.Database.Models.Users;
using RefreshTests.GameServer.Extensions;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Notifications;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;
using Refresh.Interfaces.Game.Endpoints.DataTypes.Request;
using Refresh.Interfaces.Game.Endpoints.DataTypes.Response;
using Refresh.Interfaces.Game.Types.Levels;

namespace RefreshTests.GameServer.Tests.Levels;

public class PublishEndpointsTests : GameServerTest
{
    private const string TEST_ASSET_HASH = "acddf3f9251c1ddb675ad81ba34ba16135b54aca";
    private const string TEST_MISSING_ASSET_HASH = "acddf3f9251c1ddb675ad81ba34ba16135b54acb";
    
    [Test]
    public void PublishLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        GameLevelRequest level = new()
        {
            Title = "TEST LEVEL",
            IconHash = "g719",
            Description = "DESCRIPTION",
            Location = new GameLocation(),
            RootResource = TEST_ASSET_HASH,
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedLevelResources resourcesToUpload = message.Content.ReadAsXML<SerializedLevelResources>();
        Assert.That(resourcesToUpload.Resources, Has.Length.EqualTo(1));
        Assert.That(resourcesToUpload.Resources[0], Is.EqualTo(TEST_ASSET_HASH));

        //Upload our """level"""
        message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent("LVLb"u8.ToArray())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        GameLevelResponse response = message.Content.ReadAsXML<GameLevelResponse>();
        Assert.That(response.Title, Is.EqualTo(level.Title));
        Assert.That(response.Description, Is.EqualTo(level.Description));

        level.LevelId = response.LevelId;
        level.Title = "REPUBLISH!";
        level.Description = "REPUBLISHED!!!!";
        
        message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        //Since theres no new assets, the XML deserializer will deserialize the resources list into null
        resourcesToUpload = message.Content.ReadAsXML<SerializedLevelResources>();
        Assert.That(resourcesToUpload.Resources, Is.EqualTo(null));

        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        response = message.Content.ReadAsXML<GameLevelResponse>();
        Assert.That(response.Title, Is.EqualTo(level.Title));
        Assert.That(response.Description, Is.EqualTo(level.Description));
    }
    
    [Test]
    public void LevelWithLongTitleGetsTruncated()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        GameLevelRequest level = new()
        {
            Title = new string('*', UgcLimits.TitleLimit * 2),
            IconHash = "g0",
            Description = "Normal length",
            Location = new GameLocation(),
            RootResource = TEST_ASSET_HASH,
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //Upload our """level"""
        message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent("LVLb"u8.ToArray())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        Assert.That(message.Content.ReadAsXML<GameLevelResponse>().Title.Length, Is.EqualTo(UgcLimits.TitleLimit));
    }
    
    [Test]
    public void LevelWithLongDescriptionGetsTruncated()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        GameLevelRequest level = new()
        {
            Title = "Normal Title!",
            IconHash = "g0",
            Description = new string('=', UgcLimits.DescriptionLimit * 2),
            Location = new GameLocation(),
            RootResource = TEST_ASSET_HASH,
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        //Upload our """level"""
        message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent("LVLb"u8.ToArray())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        Assert.That(message.Content.ReadAsXML<GameLevelResponse>().Description.Length, Is.EqualTo(UgcLimits.DescriptionLimit));
    }

    [Test]
    public void CantPublishLevelWithInvalidMaxPlayers()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        GameLevelRequest level = new()
        {
            Title = "Normal Title!",
            IconHash = "g0",
            Description = "Normal Description",
            Location = new GameLocation(),
            RootResource = TEST_ASSET_HASH,
            MinPlayers = 1,
            MaxPlayers = 5,
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }
    
    [Test]
    public void CantPublishLevelWithInvalidMinPlayers()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        GameLevelRequest level = new()
        {
            Title = "Normal Title!",
            IconHash = "g0",
            Description = "Normal Description",
            Location = new GameLocation(),
            RootResource = TEST_ASSET_HASH,
            MinPlayers = -1,
            MaxPlayers = 4,
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }
    
    [Test]
    public void CantPublishLevelWithInvalidRootResource()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        GameLevelRequest level = new()
        {
            Title = "Normal Title!",
            IconHash = "g0",
            Description = "Normal Description",
            Location = new GameLocation(),
            RootResource = "I AM INVALID!!!",
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }
    
    [TestCase(TokenGame.LittleBigPlanet1)]
    [TestCase(TokenGame.LittleBigPlanet2)]
    [TestCase(TokenGame.LittleBigPlanet3)]
    [TestCase(TokenGame.LittleBigPlanetVita)]
    public void CantPublishLevelWithInvalidIconGuid(TokenGame game)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, game, TokenPlatform.PS3, user);

        GameLevelRequest level = new()
        {
            Title = "Normal Title!",
            IconHash = "g0",
            Description = "Normal Description",
            Location = new GameLocation(),
            RootResource = "I AM INVALID!!!",
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }
    
    public void CanPublishLevelWithInvalidIconGuidPsp()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanetPSP, TokenPlatform.PS3, user);

        GameLevelRequest level = new()
        {
            Title = "Normal Title!",
            IconHash = "g0",
            Description = "Normal Description",
            Location = new GameLocation(),
            RootResource = "I AM INVALID!!!",
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
    }
    
    [Test]
    public void CantPublishLevelWithMissingRootResource()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        GameLevelRequest level = new()
        {
            Title = "Normal Title!",
            IconHash = "g719",
            Description = "Normal Description",
            Location = new GameLocation(),
            RootResource = TEST_MISSING_ASSET_HASH,
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }

    [Test]
    public void CantUpdateNonExistentLevel()
    {
        using TestContext context = this.GetServer();
        GameUser author = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, author);

        GameLevelRequest level = new()
        {
            LevelId = 69696969,
            Title = "TEST LEVEL",
            IconHash = "g719",
            Description = "DESCRIPTION",
            Location = new GameLocation(),
            RootResource = TEST_ASSET_HASH,
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedLevelResources resourcesToUpload = message.Content.ReadAsXML<SerializedLevelResources>();
        Assert.That(resourcesToUpload.Resources, Has.Length.EqualTo(1));
        Assert.That(resourcesToUpload.Resources[0], Is.EqualTo(TEST_ASSET_HASH));

        //Upload our """level"""
        message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent("LVLb"u8.ToArray())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        // Make sure this request fails and there are no levels uploaded
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
        Assert.That(context.Database.GetTotalLevelCount(), Is.Zero);
    }
    
    [Test]
    public void CantRepublishOtherUsersLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();

        using HttpClient client1 = context.GetAuthenticatedClient(TokenType.Game, user1);
        using HttpClient client2 = context.GetAuthenticatedClient(TokenType.Game, user2);

        GameLevelRequest level = new()
        {
            Title = "TEST LEVEL",
            IconHash = "g719",
            Description = "DESCRIPTION",
            Location = new GameLocation(),
            RootResource = TEST_ASSET_HASH
        };

        HttpResponseMessage message = client1.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedLevelResources resourcesToUpload = message.Content.ReadAsXML<SerializedLevelResources>();
        Assert.That(resourcesToUpload.Resources, Has.Length.EqualTo(1));
        Assert.That(resourcesToUpload.Resources[0], Is.EqualTo(TEST_ASSET_HASH));

        //Upload our """level"""
        message = client1.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent("LVLb"u8.ToArray())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //As user 1, publish a level
        message = client1.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        GameLevelResponse response = message.Content.ReadAsXML<GameLevelResponse>();
        Assert.That(response.Title, Is.EqualTo(level.Title));
        Assert.That(response.Description, Is.EqualTo(level.Description));

        level.LevelId = response.LevelId;
        level.Title = "REPUBLISH!";
        level.Description = "REPUBLISHED!!!! HEEHEEHEE IM GONNA CHANGE YOUR LEVEL TO SAY BAD THINGS";
        
        //As user 2, try to republish over user 1's level
        message = client2.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }
    
    [Test]
    public void CantPublishSameRootLevelHashTwice()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();

        using HttpClient client1 = context.GetAuthenticatedClient(TokenType.Game, user1);
        using HttpClient client2 = context.GetAuthenticatedClient(TokenType.Game, user2);

        GameLevelRequest level = new()
        {
            Title = "TEST LEVEL",
            IconHash = "g719",
            Description = "DESCRIPTION",
            Location = new GameLocation(),
            RootResource = TEST_ASSET_HASH,
        };

        HttpResponseMessage message = client1.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedLevelResources resourcesToUpload = message.Content.ReadAsXML<SerializedLevelResources>();
        Assert.That(resourcesToUpload.Resources, Has.Length.EqualTo(1));
        Assert.That(resourcesToUpload.Resources[0], Is.EqualTo(TEST_ASSET_HASH));

        //Upload our """level"""
        message = client1.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent("LVLb"u8.ToArray())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //As user 1, publish a level
        message = client1.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        GameLevelResponse response = message.Content.ReadAsXML<GameLevelResponse>();
        Assert.That(response.Title, Is.EqualTo(level.Title));
        Assert.That(response.Description, Is.EqualTo(level.Description));

        level.Title = "REPUBLISH!";
        level.Description = "PANA KIN!!!! MI PAIN PEKO E SINA";
        
        //As user 2, try to publish a level with the same root hash
        message = client2.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }
    
    [Test]
    public void UnpublishLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message = client.PostAsync($"/lbp/unpublish/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        DatabaseList<GameLevel> levelsByUser = context.Database.GetLevelsByUser(user, 1, 0, new LevelFilterSettings(TokenGame.LittleBigPlanet3), user);
        Assert.That(levelsByUser.TotalItems, Is.EqualTo(0));
    }

    [Test]
    public void CantUnpublishInvalidLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message = client.PostAsync($"/lbp/unpublish/{int.MaxValue}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void CantUnpublishSomeoneElsesLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user1);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user2);

        HttpResponseMessage message = client.PostAsync($"/lbp/unpublish/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(Unauthorized));

        DatabaseList<GameLevel> levelsByUser = context.Database.GetLevelsByUser(user1, 1, 0, new LevelFilterSettings(TokenGame.LittleBigPlanet3), user1);
        Assert.That(levelsByUser.TotalItems, Is.EqualTo(1));
    }

    [Test]
    [TestCase(2, 2)]
    [Ignore("needs to change lvl hash every iteration")] // TODO
    public void CantPublishAfterExceedingTimedLevelLimit(int levelQuota, int uploadAttemptsAfterExceeding)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser("thepublisher");

        // Prepare config
        GameServerConfig config = context.Server.Value.GameServerConfig;
        config.TimedLevelUploadLimits = new()
        {
            Enabled = true,
            LevelQuota = levelQuota,
            TimeSpanHours = 1,
        };

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        // Upload level asset
        HttpResponseMessage assetUploadMessage = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent("LVLb"u8.ToArray())).Result;
        Assert.That(assetUploadMessage.StatusCode, Is.EqualTo(OK));

        // Fill up quota
        SpamSuccessfulUploads(config.TimedLevelUploadLimits.LevelQuota, client);
        
        // Try to upload more levels after exceeding quota
        for (int i = 0; i < uploadAttemptsAfterExceeding; i++)
        {
            GameLevelRequest level = CreateValidTestLevel(i + 1);

            HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
            Assert.That(message.StatusCode, Is.EqualTo(Unauthorized));
            message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
            Assert.That(message.StatusCode, Is.EqualTo(Unauthorized));
        }

        // Check amount of levels
        DatabaseList<GameLevel> levelsByUser = context.Database.GetLevelsByUser(user, 1000, 0, new LevelFilterSettings(TokenGame.LittleBigPlanet3), user);
        Assert.That(levelsByUser.TotalItems, Is.EqualTo(config.TimedLevelUploadLimits.LevelQuota));

        // Ensure there were error notifications sent for each blocked request to both /startPublish and /publish
        DatabaseList<GameNotification> newNotifications = context.Database.GetNotificationsByUser(user, 1000, 0);
        Assert.That(newNotifications.TotalItems, Is.EqualTo(uploadAttemptsAfterExceeding * 2));
    }

    [Test]
    [TestCase(2, 2)]
    [Ignore("needs to change lvl hash every iteration")] // TODO
    public void ResetTimedLevelLimitAfterExpiry(int levelQuota, int uploadAttemptsAfterExceeding)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser("thepublisher");

        // Prepare config
        GameServerConfig config = context.Server.Value.GameServerConfig;
        config.TimedLevelUploadLimits = new()
        {
            Enabled = true,
            LevelQuota = levelQuota,
            // Having this be 0 causes the server to always set the expiry date to now, making the expiry date be not null but always expired,
            // causing both /startPublish and /publish to always reset the limit after setting it in a previous /publish request, and allowing publish requests
            TimeSpanHours = 0,
        };

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        // Upload level asset
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent("LVLb"u8.ToArray())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Fill up quota
        SpamSuccessfulUploads(config.TimedLevelUploadLimits.LevelQuota, client);
        
        // Try to upload more levels after exceeding quota
        SpamSuccessfulUploads(uploadAttemptsAfterExceeding, client);

        // Check amount of levels
        DatabaseList<GameLevel> levelsByUser = context.Database.GetLevelsByUser(user, 1000, 0, new LevelFilterSettings(TokenGame.LittleBigPlanet3), user);
        Assert.That(levelsByUser.TotalItems, Is.EqualTo(config.TimedLevelUploadLimits.LevelQuota + uploadAttemptsAfterExceeding));

        // Ensure there were no notifications sent
        DatabaseList<GameNotification> newNotifications = context.Database.GetNotificationsByUser(user, 1000, 0);
        Assert.That(newNotifications.TotalItems, Is.EqualTo(0));
    }

    private void SpamSuccessfulUploads(int uploads, HttpClient client)
    {
        for (int i = 0; i < uploads; i++)
        {
            GameLevelRequest level = CreateValidTestLevel(i + 1);

            // Upload level
            HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
            Assert.That(message.StatusCode, Is.EqualTo(OK));
            message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
            Assert.That(message.StatusCode, Is.EqualTo(OK));
        }
    }

    private GameLevelRequest CreateValidTestLevel(int id)
        => new()
        {
            Title = "Test level! " + id,
            IconHash = "g0",
            Description = "Test description",
            Location = new GameLocation(),
            RootResource = TEST_ASSET_HASH,
        };

    [Test]
    public void CanPublishLevelWithSkillRewards()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanetVita, TokenPlatform.Vita, user);

        GameLevelRequest level = new()
        {
            LevelId = 0,
            IsAdventure = false,
            Title = "Normal Title!",
            IconHash = "g0",
            Description = new string('=', UgcLimits.DescriptionLimit * 2),
            Location = new GameLocation(),
            RootResource = TEST_ASSET_HASH,
            SkillRewards = new List<GameSkillReward>()
            {
                new()
                {
                    ConditionType = GameSkillRewardCondition.Lives,
                    RequiredAmount = 3,
                    Id = 1,
                    Title = "do the stuff",
                    Enabled = true,
                },
            },
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent("LVLb"u8.ToArray())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        GameLevelResponse resp = message.Content.ReadAsXML<GameLevelResponse>();
        Assert.That(resp, Is.Not.Null);
        Assert.That(resp.Description, Has.Length.EqualTo(UgcLimits.DescriptionLimit));
        
        Assert.That(resp.SkillRewards, Has.Count.EqualTo(1));
        Assert.That(resp.SkillRewards.First().Title, Is.EqualTo("do the stuff"));
        
        context.Database.Refresh();
        GameLevel? dbLevel = context.Database.GetLevelById(resp.LevelId);
        Assert.That(dbLevel, Is.Not.Null);

        IEnumerable<GameSkillReward> rewards = context.Database.GetSkillRewardsForLevel(dbLevel!);
        Assert.That(rewards, Is.Not.Empty);
    }
    
    [Test]
    public void ReuploadStatusPreserved()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent("LVLb"u8.ToArray())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        GameLevel dbLevel = context.CreateLevel(user);
        context.Database.UpdateLevel(new ApiAdminEditLevelRequest()
        {
            IsReUpload = true,
            OriginalPublisher = "glotchmeister69",
        }, dbLevel, null);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dbLevel.IsReUpload, Is.True);
            Assert.That(dbLevel.OriginalPublisher, Is.EqualTo("glotchmeister69"));
        }

        GameLevelRequest level = new()
        {
            LevelId = dbLevel.LevelId,
            IsAdventure = false,
            Title = "Level",
            IconHash = "0",
            Description = new string('=', UgcLimits.DescriptionLimit * 2),
            Location = new GameLocation(),
            RootResource = TEST_ASSET_HASH,
            SkillRewards = new List<GameSkillReward>()
            {
                new()
                {
                    ConditionType = GameSkillRewardCondition.Lives,
                    RequiredAmount = 3,
                    Id = 1,
                    Title = "do the stuff",
                    Enabled = true,
                },
            },
        };
        
        // Republish the level
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        context.Database.Refresh();
        dbLevel = context.Database.GetLevelById(dbLevel.LevelId)!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dbLevel.IsReUpload, Is.True);
            Assert.That(dbLevel.OriginalPublisher, Is.EqualTo("glotchmeister69"));
        }
    }
}