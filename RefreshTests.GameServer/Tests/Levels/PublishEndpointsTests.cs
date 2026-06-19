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
using System.Security.Cryptography;
using System.Text;
using System.Net;
using Refresh.Database.Models.Assets;
using Bunkum.Core.Storage;

namespace RefreshTests.GameServer.Tests.Levels;

public class PublishEndpointsTests : GameServerTest
{
    private const string TEST_ASSET_HASH = "acddf3f9251c1ddb675ad81ba34ba16135b54aca";
    private const string TEST_MISSING_ASSET_HASH = "acddf3f9251c1ddb675ad81ba34ba16135b54acb";

    private string UploadLevelAsset(IDataStore dataStore)
    {
        ReadOnlySpan<byte> data = "LVLb"u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();
        dataStore.WriteToStore(hash, data);
        return hash;
    }
    
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

    [Test]
    public void CantPublishLevelWithDisallowedRootResource()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        ReadOnlySpan<byte> data = "LVLb"u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();
        context.GetDataStore().WriteToStore(hash, data);

        context.Database.DisallowAsset(hash, GameAssetType.Level, "too many bs obstacles");
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        GameLevelRequest level = new()
        {
            Title = "Boom Town 2",
            RootResource = hash,
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(Unauthorized));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(Unauthorized));
    }

    [Test]
    public void CantPublishLevelWithNoRootResource()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        GameLevelRequest level = new()
        {
            Title = "The best level you've ever played !!!",
            RootResource = "0",
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
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
            IconHash = "g1087",
            RootResource = this.UploadLevelAsset(context.GetDataStore()),
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }
    
    [Test]
    public void CantPublishLevelWithInvalidIconGuidPsp()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanetPSP, TokenPlatform.PS3, user);

        GameLevelRequest level = new()
        {
            Title = "Normal Title!",
            IconHash = "g67", // max is g63
            RootResource = this.UploadLevelAsset(context.GetDataStore()),
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }

    [Test]
    public void CantPublishLevelWithMissingCustomIcon()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        ReadOnlySpan<byte> data = "TEX "u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();
        // Don't write to store

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        GameLevelRequest level = new()
        {
            Title = "67",
            IconHash = hash,
            RootResource = this.UploadLevelAsset(context.GetDataStore()),
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void CantPublishLevelWithDisallowedIcon()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        ReadOnlySpan<byte> data = "TEX "u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();
        context.GetDataStore().WriteToStore(hash, data);

        context.Database.DisallowAsset(hash, GameAssetType.Texture, "too cringe");
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        GameLevelRequest level = new()
        {
            Title = "67",
            IconHash = hash,
            RootResource = this.UploadLevelAsset(context.GetDataStore()),
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(Unauthorized));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(Unauthorized));
    }

    [Test]
    [TestCase(TokenGame.LittleBigPlanet1, false)]
    [TestCase(TokenGame.LittleBigPlanetPSP, false)]
    [TestCase(TokenGame.LittleBigPlanet3, true)]
    [TestCase(TokenGame.BetaBuild, true)]
    public void TestAdventureUploadsFromVariousGames(TokenGame game, bool success)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        ReadOnlySpan<byte> data = "ADCb"u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();
        context.GetDataStore().WriteToStore(hash, data);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, game, TokenPlatform.PS3, user);

        GameLevelRequest level = new()
        {
            Title = "adventure time!",
            IsAdventure = true,
            Description = "epic joke please laugh",
            RootResource = hash,
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(success ? OK : Unauthorized));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(success ? OK : Unauthorized));
    }

    [Test]
    [TestCase("LVLb", false)]
    [TestCase("PLNb", false)]
    [TestCase("CHKb", false)]
    [TestCase("TEX ", false)]
    [TestCase("ADCb", true)]
    public void TestAdventureRootResourceTypes(string resource, bool success)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        ReadOnlySpan<byte> data = new(Encoding.UTF8.GetBytes(resource));
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();
        context.GetDataStore().WriteToStore(hash, data);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);

        GameLevelRequest level = new()
        {
            Title = "totally an adventure!",
            IsAdventure = true,
            RootResource = hash,
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(success ? OK : BadRequest));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(success ? OK : BadRequest));
    }

    [Test]
    [TestCase("LVLb", true)]
    [TestCase("PLNb", false)]
    [TestCase("CHKb", false)]
    [TestCase("TEX ", false)]
    [TestCase("ADCb", false)]
    public void TestLevelRootResourceTypes(string resource, bool success)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        ReadOnlySpan<byte> data = new(Encoding.UTF8.GetBytes(resource));
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();
        context.GetDataStore().WriteToStore(hash, data);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);

        GameLevelRequest level = new()
        {
            Title = "totally a level!",
            IsAdventure = false,
            RootResource = hash,
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(success ? OK : BadRequest));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(success ? OK : BadRequest));
    }

    [Test]
    [TestCase("LVLb")]
    [TestCase("PLNb")]
    [TestCase("CHKb")]
    [TestCase("TEX ")]
    [TestCase("wasedrthzjtgerw")]
    public void LevelRootResourceTypeIsIgnoredIfPSP(string resource)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        ReadOnlySpan<byte> data = new(Encoding.UTF8.GetBytes(resource));
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();
        context.GetDataStore().WriteToStore($"psp/{hash}", data);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanetPSP, TokenPlatform.PSP, user);
        client.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");

        GameLevelRequest level = new()
        {
            Title = "totally a level!",
            IsAdventure = false,
            RootResource = hash,
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
    }

    [Test]
    public void CantPublishRegularLevelWithInnerLevels()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        ReadOnlySpan<byte> data = "LVLb"u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();
        context.GetDataStore().WriteToStore(hash, data);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);

        GameLevelRequest level = new()
        {
            Title = "totally an adventure!",
            IsAdventure = false,
            RootResource = hash,
            Slots =
            [
                new()
                {
                    Title = "Hi lol",
                    RootResource = "",
                }
            ]
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }

    [Test]
    public void CanPublishAdventureWithInnerLevels()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        ReadOnlySpan<byte> data = "ADCb"u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();
        context.GetDataStore().WriteToStore(hash, data);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);

        GameLevelRequest level = new()
        {
            Title = "totally an adventure!",
            IsAdventure = true,
            RootResource = hash,
            Slots =
            [
                new()
                {
                    Title = "Hi lol",
                    RootResource = "",
                }
            ]
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
    }

    [Test]
    public void CantPublishAdventureWithRecursiveInnerLevels()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        ReadOnlySpan<byte> data = "ADCb"u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();
        context.GetDataStore().WriteToStore(hash, data);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);

        GameLevelRequest level = new()
        {
            Title = "Deep adventure",
            IsAdventure = true,
            RootResource = hash,
            Slots =
            [
                new()
                {
                    Title = "Hi lol",
                    IsAdventure = false,
                    RootResource = "",
                    Slots = 
                    [
                        new()
                        {
                            Title = "Super secret",
                            IsAdventure = false,
                            RootResource = "",
                            Slots = 
                            [
                                new()
                                {
                                    Title = "This is getting ridiculous...",
                                    IsAdventure = false,
                                    RootResource = "",
                                }
                            ]
                        }
                    ]
                }
            ]
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }

    [Test]
    public void CantPublishAdventureWithInnerAdventures()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        ReadOnlySpan<byte> data = "ADCb"u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();
        context.GetDataStore().WriteToStore(hash, data);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);

        GameLevelRequest level = new()
        {
            Title = "Very adventurous",
            IsAdventure = true,
            RootResource = hash,
            Slots =
            [
                new()
                {
                    Title = "Hi lol",
                    IsAdventure = true,
                    RootResource = "",
                }
            ]
        };

        HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
        
        message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
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
    public void CantOverwriteNonLbp3LevelInLbp3()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        using HttpClient clientLbp3 = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);

        GameLevel level = context.Database.AddLevel(new GameLevelRequest()
        {
            Title = "AMASING LBP1 LEVEL!!",
            Description = "Im gonna come up with a description later...",
            RootResource = "epic",
        }, TokenGame.LittleBigPlanet1, user);

        // Now try republishing the level in LBP3
        GameLevelRequest republish = new()
        {
            LevelId = level.LevelId,
            Title = "AMAZING LBP1 LEVEL!!",
            IconHash = "g719",
            Description = "THIS LEVEL IS SO GOOD IT MAKES ME WANT TO BEAUTIFY ITS METADATA IN LBP3 WITHOUT LOOKING!!",
            RootResource = TEST_ASSET_HASH,
        };

        // Upload our """level"""
        HttpResponseMessage message = clientLbp3.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent("LVLb"u8.ToArray())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        message = clientLbp3.PostAsync("/lbp/publish", new StringContent(republish.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        GameLevelResponse response = message.Content.ReadAsXML<GameLevelResponse>();
        Assert.That(response.Title, Is.EqualTo(republish.Title));
        Assert.That(response.Description, Is.EqualTo(republish.Description));
        Assert.That(response.IconHash, Is.EqualTo(republish.IconHash));
        Assert.That(response.RootResource, Is.EqualTo("epic"));
        Assert.That(response.GameVersion, Is.EqualTo((int)TokenGame.LittleBigPlanet1));
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

        DatabaseList<GameLevel> levelsByUser = context.Database.GetLevelsByUser(user, 1, 0, new(TokenGame.LittleBigPlanet3), user);
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

        DatabaseList<GameLevel> levelsByUser = context.Database.GetLevelsByUser(user1, 1, 0, new(TokenGame.LittleBigPlanet3), user1);
        Assert.That(levelsByUser.TotalItems, Is.EqualTo(1));
    }

    [Test]
    [TestCase(4, 4, 1)]
    public void LevelUploadsGetRateLimitedTemporarily(int levelQuota, int uploadAttemptsAfterExceeding, int timeSpanHours)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser("thepublisher");

        // Prepare config
        GameServerConfig config = context.Server.Value.GameServerConfig;
        EntityUploadRateLimitProperties uploadConfig = new()
        {
            Enabled = true,
            UploadQuota = levelQuota,
            TimeSpanHours = timeSpanHours,
        };
        config.NormalUserPermissions.LevelUploadRateLimit = uploadConfig;

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        int publishAttempts = 0;

        // Not blocked yet
        Assert.That(context.Database.GetUploadRateLimit(user, GameDatabaseEntity.Level), Is.Null);
        Assert.That(context.Database.GetRemainingTimeIfUploadRateLimitReached(user, GameDatabaseEntity.Level, uploadConfig.UploadQuota), Is.Null);

        // Fill up half of quota
        publishAttempts += SpamPublishUniqueLevels(uploadConfig.UploadQuota / 2, client, publishAttempts);
        context.Database.Refresh();

        // There is rate-limit data in DB, but the rate-limit hasn't been triggered yet
        EntityUploadRateLimit? uploadLimit = context.Database.GetUploadRateLimit(user, GameDatabaseEntity.Level);
        Assert.That(uploadLimit, Is.Not.Null);
        Assert.That(uploadLimit!.UploadCount, Is.EqualTo(uploadConfig.UploadQuota / 2));
        Assert.That(context.Database.GetRemainingTimeIfUploadRateLimitReached(user, GameDatabaseEntity.Level, uploadConfig.UploadQuota), Is.Null);
        
        // Try to upload more levels to reach quota
        publishAttempts += SpamPublishUniqueLevels(uploadConfig.UploadQuota / 2, client, publishAttempts);
        context.Database.Refresh();

        // Ensure there were no notifications sent
        Assert.That(context.Database.GetNotificationCountByUser(user), Is.Zero);

        // There is rate-limit data in DB, and the rate-limit has been triggered
        uploadLimit = context.Database.GetUploadRateLimit(user, GameDatabaseEntity.Level);
        Assert.That(uploadLimit, Is.Not.Null);
        Assert.That(uploadLimit!.UploadCount, Is.EqualTo(uploadConfig.UploadQuota));
        Assert.That(context.Database.GetRemainingTimeIfUploadRateLimitReached(user, GameDatabaseEntity.Level, uploadConfig.UploadQuota), Is.Not.Null);

        // levels are blocked
        publishAttempts += SpamPublishUniqueLevels(uploadAttemptsAfterExceeding, client, publishAttempts, Unauthorized);
        context.Database.Refresh();

        // Check amount of levels, and ensure there were notifications sent for every failed level
        Assert.That(context.Database.GetTotalLevelsByUser(user), Is.EqualTo(uploadConfig.UploadQuota));
        Assert.That(context.Database.GetNotificationCountByUser(user), Is.EqualTo(uploadAttemptsAfterExceeding * 2));

        // Expire limit naturally by trying to publish again later
        context.Time.TimestampMilliseconds = 1000 * 60 * 60 * timeSpanHours + 10;
        publishAttempts += SpamPublishUniqueLevels(uploadAttemptsAfterExceeding, client, publishAttempts);
        context.Database.Refresh();

        // there are more levels now, and no new notifications
        Assert.That(context.Database.GetTotalLevelsByUser(user), Is.EqualTo(uploadConfig.UploadQuota * 2));
        Assert.That(context.Database.GetNotificationCountByUser(user), Is.EqualTo(uploadAttemptsAfterExceeding * 2));
    }

    private int SpamPublishUniqueLevels(int uploads, HttpClient client, int startIndex, HttpStatusCode expectedStatus = OK)
    {
        for (int i = 0; i < uploads; i++)
        {
            // Upload level
            GameLevelRequest level = PrepareUniqueLevelPublishRequest(client, startIndex + i);

            HttpResponseMessage message = client.PostAsync("/lbp/startPublish", new StringContent(level.AsXML())).Result;
            Assert.That(message.StatusCode, Is.EqualTo(expectedStatus));
            message = client.PostAsync("/lbp/publish", new StringContent(level.AsXML())).Result;
            Assert.That(message.StatusCode, Is.EqualTo(expectedStatus));
        }

        return uploads;
    }

    private GameLevelRequest PrepareUniqueLevelPublishRequest(HttpClient client, int uniqueValue)
    {
        // upload root asset
        ReadOnlySpan<byte> rootResource = new(Encoding.ASCII.GetBytes($"LVLb {uniqueValue}"));
        string rootHash = BitConverter.ToString(SHA1.HashData(rootResource)).Replace("-", "").ToLower();

        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{rootHash}", new ReadOnlyMemoryContent(rootResource.ToArray())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // prepare request body
        return new()
        {
            Title = "Test level! " + uniqueValue,
            Description = "Test description",
            RootResource = rootHash,
        };
    }

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