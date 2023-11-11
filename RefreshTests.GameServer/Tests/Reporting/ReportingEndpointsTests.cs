using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
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
    public void CantUploadReportWithBadLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, user);

        GameReport report = new()
        {
            LevelId = int.MaxValue,
        };
        
        HttpResponseMessage response = client.PostAsync("/lbp/grief", new StringContent(report.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(BadRequest));
    }
    
    [Test]
    public void CantUploadReportWithBadPlayerCount()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, user);

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
        Assert.That(response.StatusCode, Is.EqualTo(BadRequest));
    }
    
    [Test]
    public void CantUploadReportWithBadScreenElementPlayerCount()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, user);

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
        Assert.That(response.StatusCode, Is.EqualTo(BadRequest));
    }
}