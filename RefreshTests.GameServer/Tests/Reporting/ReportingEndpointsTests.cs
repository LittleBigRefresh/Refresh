using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Photos;
using Refresh.GameServer.Types.Report;
using Refresh.GameServer.Types.UserData;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Reporting;

public class ReportingEndpointsTests : GameServerTest
{
    [Test]
    public void UploadReport()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, user);

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
            JpegHash = null,
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

        DatabaseList<GameReport> griefReports = context.Database.GetGriefReports(10, 0);
        Assert.That(griefReports.TotalItems, Is.EqualTo(1));
        List<GameReport> reports = griefReports.Items.ToList();
        Assert.That(reports[0].Description, Is.EqualTo(report.Description));
        Assert.That(reports[0].LevelId, Is.EqualTo(report.LevelId));
    }
    
    [Test]
    public void CanUploadReportWithBadLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, user);

        GameReport report = new()
        {
            LevelId = int.MaxValue,
        };
        
        HttpResponseMessage response = client.PostAsync("/lbp/grief", new StringContent(report.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));
        
        context.Database.Refresh();

        DatabaseList<GameReport> griefReports = context.Database.GetGriefReports(10, 0);
        Assert.That(griefReports.TotalItems, Is.EqualTo(1));
        List<GameReport> reports = griefReports.Items.ToList();
        Assert.That(reports[0].Description, Is.EqualTo(report.Description));
    }
    
    [TestCase(true)]
    [TestCase(false)]
    public void CantUploadReportWithBadPlayerCount(bool psp)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, user);
        if(psp) client.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");
        
        GameReport report = new()
        {
            LevelId = 0,
            Players = new Player[]
            {
                new()
                {
                    Username = "hee",
                },
                new()
                {
                    Username = "haw",
                },
                new()
                {
                    Username = "ham",
                },
                new()
                {
                    Username = "burg",
                },
                new()
                {
                    Username = "er",
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
        
        GameReport report = new()
        {
            LevelId = 0,
            ScreenElements = new ScreenElements
            {
                Player = new Player[]
                {
                    new()
                    {
                        Username = "hee",
                    },
                    new()
                    {
                        Username = "haw",
                    },
                    new()
                    {
                        Username = "ham",
                    },
                    new()
                    {
                        Username = "burg",
                    },
                    new()
                    {
                        Username = "er",
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
        
        //Enable the grief report re-direction
        context.Database.SetUserGriefReportRedirection(user, true);
        context.Database.Refresh();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, game, TokenPlatform.PS3, user);

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
        
        //Enable the grief report re-direction
        context.Database.SetUserGriefReportRedirection(user, true);
        context.Database.Refresh();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, game, TokenPlatform.PS3, user);

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
        
        //Enable the grief report re-direction
        context.Database.SetUserGriefReportRedirection(user, true);
        context.Database.Refresh();

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