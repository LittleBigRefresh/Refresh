using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.Game.DataTypes.Request;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Types;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Levels.SkillRewards;
using Refresh.GameServer.Types.UserData;
using RefreshTests.GameServer.Extensions;

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
            LevelId = 0,
            Title = "TEST LEVEL",
            IconHash = "g719",
            Description = "DESCRIPTION",
            Location = new GameLocation(),
            GameVersion = 0,
            RootResource = TEST_ASSET_HASH,
            PublishDate = 0,
            UpdateDate = 0,
            MinPlayers = 0,
            MaxPlayers = 0,
            EnforceMinMaxPlayers = false,
            SameScreenGame = false,
            SkillRewards = new List<GameSkillReward>(),
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
    public void CantPublishLevelWithInvalidTitle()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        GameLevelRequest level = new()
        {
            LevelId = 0,
            Title = new string('*', 500),
            IconHash = "g0",
            Description = "Normal length",
            Location = new GameLocation(),
            GameVersion = 0,
            RootResource = TEST_ASSET_HASH,
            PublishDate = 0,
            UpdateDate = 0,
            MinPlayers = 0,
            MaxPlayers = 0,
            EnforceMinMaxPlayers = false,
            SameScreenGame = false,
            SkillRewards = new List<GameSkillReward>(),
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }
    
    [Test]
    public void CantPublishLevelWithInvalidDescription()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        GameLevelRequest level = new()
        {
            LevelId = 0,
            Title = "Normal Title!",
            IconHash = "g0",
            Description = new string('=', 5000),
            Location = new GameLocation(),
            GameVersion = 0,
            RootResource = TEST_ASSET_HASH,
            PublishDate = 0,
            UpdateDate = 0,
            MinPlayers = 0,
            MaxPlayers = 0,
            EnforceMinMaxPlayers = false,
            SameScreenGame = false,
            SkillRewards = new List<GameSkillReward>(),
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }
    
    [Test]
    public void CantPublishLevelWithInvalidMaxPlayers()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        GameLevelRequest level = new()
        {
            LevelId = 0,
            Title = "Normal Title!",
            IconHash = "g0",
            Description = "Normal Description",
            Location = new GameLocation(),
            GameVersion = 0,
            RootResource = TEST_ASSET_HASH,
            PublishDate = 0,
            UpdateDate = 0,
            MinPlayers = 0,
            MaxPlayers = 5,
            EnforceMinMaxPlayers = false,
            SameScreenGame = false,
            SkillRewards = new List<GameSkillReward>(),
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
            LevelId = 0,
            Title = "Normal Title!",
            IconHash = "g0",
            Description = "Normal Description",
            Location = new GameLocation(),
            GameVersion = 0,
            RootResource = TEST_ASSET_HASH,
            PublishDate = 0,
            UpdateDate = 0,
            MinPlayers = -1,
            MaxPlayers = 0,
            EnforceMinMaxPlayers = false,
            SameScreenGame = false,
            SkillRewards = new List<GameSkillReward>(),
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
            LevelId = 0,
            Title = "Normal Title!",
            IconHash = "g0",
            Description = "Normal Description",
            Location = new GameLocation(),
            GameVersion = 0,
            RootResource = "I AM INVALID!!!",
            PublishDate = 0,
            UpdateDate = 0,
            MinPlayers = 0,
            MaxPlayers = 0,
            EnforceMinMaxPlayers = false,
            SameScreenGame = false,
            SkillRewards = new List<GameSkillReward>(),
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
            LevelId = 0,
            Title = "Normal Title!",
            IconHash = "g0",
            Description = "Normal Description",
            Location = new GameLocation(),
            GameVersion = 0,
            RootResource = "I AM INVALID!!!",
            PublishDate = 0,
            UpdateDate = 0,
            MinPlayers = 0,
            MaxPlayers = 0,
            EnforceMinMaxPlayers = false,
            SameScreenGame = false,
            SkillRewards = new List<GameSkillReward>(),
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
            LevelId = 0,
            Title = "Normal Title!",
            IconHash = "g0",
            Description = "Normal Description",
            Location = new GameLocation(),
            GameVersion = 0,
            RootResource = "I AM INVALID!!!",
            PublishDate = 0,
            UpdateDate = 0,
            MinPlayers = 0,
            MaxPlayers = 0,
            EnforceMinMaxPlayers = false,
            SameScreenGame = false,
            SkillRewards = new List<GameSkillReward>(),
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
            LevelId = 0,
            Title = "Normal Title!",
            IconHash = "g719",
            Description = "Normal Description",
            Location = new GameLocation(),
            GameVersion = 0,
            RootResource = TEST_MISSING_ASSET_HASH,
            PublishDate = 0,
            UpdateDate = 0,
            MinPlayers = 0,
            MaxPlayers = 0,
            EnforceMinMaxPlayers = false,
            SameScreenGame = false,
            SkillRewards = new List<GameSkillReward>(),
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
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
            LevelId = 0,
            Title = "TEST LEVEL",
            IconHash = "g719",
            Description = "DESCRIPTION",
            Location = new GameLocation(),
            GameVersion = 0,
            RootResource = TEST_ASSET_HASH,
            PublishDate = 0,
            UpdateDate = 0,
            MinPlayers = 0,
            MaxPlayers = 0,
            EnforceMinMaxPlayers = false,
            SameScreenGame = false,
            SkillRewards = new List<GameSkillReward>(),
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
    public void UnpublishLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message = client.PostAsync($"/lbp/unpublish/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        DatabaseList<GameLevel> levelsByUser = context.Database.GetLevelsByUser(user, 1, 0, new LevelFilterSettings(TokenGame.LittleBigPlanet3));
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

        DatabaseList<GameLevel> levelsByUser = context.Database.GetLevelsByUser(user1, 1, 0, new LevelFilterSettings(TokenGame.LittleBigPlanet3));
        Assert.That(levelsByUser.TotalItems, Is.EqualTo(1));
    }
}