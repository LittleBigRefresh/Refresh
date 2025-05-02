using System.Reflection;
using Refresh.Common.Helpers;
using Refresh.Database.Models.Authentication;
using Refresh.Database;
using Refresh.GameServer.Types.Report;
using Refresh.Database.Models.Users;
using RefreshTests.GameServer.Extensions;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Photos;

namespace RefreshTests.GameServer.Tests.Reporting;

public class ReportingEndpointsTests : GameServerTest
{
    private const string TEST_ASSET_HASH = "0ec63b140374ba704a58fa0c743cb357683313dd";
    private static readonly byte[] TestAsset = ResourceHelper.ReadResource("RefreshTests.GameServer.Resources.1x1.png", Assembly.GetExecutingAssembly());
    
    [Test]
    public void UploadReport()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, user);
        
        //Upload our """photo"""
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent(TestAsset)).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        GameReport report = new()
        {
            InfoBubble = new InfoBubble[]
                {},
            Type = GriefReportType.Obscene,
            Marqee = new Marqee
            {
                Rect = new Rect
                {
                    Top = 10,
                    Left = 10,
                    Bottom = 20,
                    Right = 20,
                },
            },
            LevelOwner = level.Publisher!.Username,
            InitialStateHash = null,
            LevelType = "user",
            LevelId = level.LevelId,
            Description = "This is a description, sent by LBP3",
            GriefStateHash = null,
            JpegHash = TEST_ASSET_HASH,
            Players = new Player[]
            {
                new()
                {
                    Username = user.Username,
                    Rectangle = new Rect
                    {
                        Top = 10,
                        Left = 10,
                        Bottom = 20,
                        Right = 20,
                    },
                    Reporter = true,
                    IngameNow = true,
                    PlayerNumber = 0,
                    Text = "Some text",
                    ScreenRect = new ScreenRect
                    {
                        Rect = new Rect
                        {
                            Top = 10,
                            Left = 10,
                            Bottom = 20,
                            Right = 20,
                        },
                    },
                },
            },
            ScreenElements = new ScreenElements(),
        };
        
        HttpResponseMessage response = client.PostAsync("/lbp/grief", new StringContent(report.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));

        context.Database.Refresh();

        DatabaseList<GamePhoto> photos = context.Database.GetRecentPhotos(10, 0);
        Assert.That(photos.TotalItems, Is.EqualTo(1));
        Assert.That(photos.Items.First().LevelId, Is.EqualTo(report.LevelId));
    }
    
    [Test]
    public void CanUploadReportWithBadLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, user);
        
        //Upload our """photo"""
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent(TestAsset)).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        GameReport report = new()
        {
            LevelId = int.MaxValue,
            JpegHash = TEST_ASSET_HASH,
        };
        
        HttpResponseMessage response = client.PostAsync("/lbp/grief", new StringContent(report.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));
        
        context.Database.Refresh();

        DatabaseList<GamePhoto> photos = context.Database.GetRecentPhotos(10, 0);
        Assert.That(photos.TotalItems, Is.EqualTo(1));
    }
    
    [TestCase(true)]
    [TestCase(false)]
    public void CantUploadReportWithBadPlayerCount(bool psp)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, user);
        if(psp) client.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");
        
        //Upload our """photo"""
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent(TestAsset)).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        GameReport report = new()
        {
            LevelId = 0,
            JpegHash = TEST_ASSET_HASH,
            Players = new Player[]
            {
                new()
                {
                    Username = "hee",
                    Rectangle = new Rect(),
                },
                new()
                {
                    Username = "haw",
                    Rectangle = new Rect(),
                },
                new()
                {
                    Username = "ham",
                    Rectangle = new Rect(),
                },
                new()
                {
                    Username = "burg",
                    Rectangle = new Rect(),
                },
                new()
                {
                    Username = "er",
                    Rectangle = new Rect(),
                },
            },
        };
        
        HttpResponseMessage response = client.PostAsync("/lbp/grief", new StringContent(report.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(psp ? OK : BadRequest));
    }
    
    [TestCase(true)]
    [TestCase(false)]
    public void CantUploadReportWithBadScreenElementPlayerCount(bool psp)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, user);
        if(psp) client.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");
        
        //Upload our """photo"""
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent(TestAsset)).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        GameReport report = new()
        {
            LevelId = 0,
            JpegHash = TEST_ASSET_HASH,
            ScreenElements = new ScreenElements
            {
                Player = new Player[]
                {
                    new()
                    {
                        Username = "hee",
                        Rectangle = new Rect(),
                    },
                    new()
                    {
                        Username = "haw",
                        Rectangle = new Rect(),
                    },
                    new()
                    {
                        Username = "ham",
                        Rectangle = new Rect(),
                    },
                    new()
                    {
                        Username = "burg",
                        Rectangle = new Rect(),
                    },
                    new()
                    {
                        Username = "er",
                        Rectangle = new Rect(),
                    },
                },
            },
        };
        
        HttpResponseMessage response = client.PostAsync("/lbp/grief", new StringContent(report.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(psp ? OK : BadRequest));
    }
    
    [TestCase(TokenGame.LittleBigPlanet1)]
    [TestCase(TokenGame.LittleBigPlanet2)]
    [TestCase(TokenGame.LittleBigPlanet3)]
    [TestCase(TokenGame.LittleBigPlanetVita)]
    [TestCase(TokenGame.LittleBigPlanetPSP)]
    public void RedirectGriefReportToPhoto(TokenGame game)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, game, TokenPlatform.PS3, user);

        //Upload our """photo"""
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent(TestAsset)).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        HttpResponseMessage response = client.PostAsync("/lbp/grief", new StringContent(new GameReport
        {
            Players = new Player[]
            {
                new()
                {
                    Username = user.Username,
                    Rectangle = new Rect
                    {
                        Top = 10,
                        Left = 10,
                        Bottom = 20,
                        Right = 20,
                    },
                },
            },
            LevelId = level.LevelId,
            JpegHash = TEST_ASSET_HASH,
        }.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));
        
        context.Database.Refresh();

        DatabaseList<GamePhoto> photos = context.Database.GetPhotosByUser(user, 10, 0);
        Assert.That(photos.TotalItems, Is.EqualTo(1));

        GamePhoto photo = photos.Items.First();
        Assert.That(photo.Publisher.UserId, Is.EqualTo(user.UserId));
    }
    
    [TestCase(TokenGame.LittleBigPlanet1)]
    [TestCase(TokenGame.LittleBigPlanet2)]
    [TestCase(TokenGame.LittleBigPlanet3)]
    [TestCase(TokenGame.LittleBigPlanetVita)]
    [TestCase(TokenGame.LittleBigPlanetPSP)]
    public void RedirectGriefReportToPhotoDeveloperLevel(TokenGame game)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.Database.GetStoryLevelById(100000);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, game, TokenPlatform.PS3, user);

        //Upload our """photo"""
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent(TestAsset)).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        HttpResponseMessage response = client.PostAsync("/lbp/grief", new StringContent(new GameReport
        {
            Players = new Player[]
            {
                new()
                {
                    Username = user.Username,
                    Rectangle = new Rect
                    {
                        Top = 10,
                        Left = 10,
                        Bottom = 20,
                        Right = 20,
                    },
                },
            },
            LevelId = level.LevelId,
            JpegHash = TEST_ASSET_HASH,
        }.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));
        
        context.Database.Refresh();

        DatabaseList<GamePhoto> photos = context.Database.GetPhotosByUser(user, 10, 0);
        Assert.That(photos.TotalItems, Is.EqualTo(1));

        GamePhoto photo = photos.Items.First();
        Assert.That(photo.Publisher.UserId, Is.EqualTo(user.UserId));
    }
    
    [Test]
    public void CantRedirectGriefReportToPhotoWhenWebsiteToken()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.Website, TokenPlatform.PS3, user);

        HttpResponseMessage response = client.PostAsync("/lbp/grief", new StringContent(new GameReport
        {
            Players = new Player[]
            {
                new()
                {
                    Username = user.Username,
                    Rectangle = new Rect
                    {
                        Top = 10,
                        Left = 10,
                        Bottom = 20,
                        Right = 20,
                    },
                },
            },
        }.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(BadRequest));
    }
}