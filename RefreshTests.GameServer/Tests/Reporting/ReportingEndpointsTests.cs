using System.Reflection;
using Refresh.Common.Helpers;
using Refresh.Database.Models.Authentication;
using Refresh.Database;
using Refresh.Database.Models.Users;
using RefreshTests.GameServer.Extensions;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Photos;
using Refresh.Database.Models.Reports;
using Refresh.Interfaces.Game.Types.Report;
using Refresh.Database.Models.Relations;

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

        SerializedGriefReport report = new()
        {
            InfoBubble = new SerializedReportInfoBubble[]
                {},
            Type = GriefReportType.TermsOfService,
            Marqee = new SerializedReportMarqee
            {
                Rect = new SerializedReportRect
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
            Players = new SerializedReportPlayer[]
            {
                new()
                {
                    Username = user.Username,
                    Rectangle = new SerializedReportRect
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
                    ScreenRect = new SerializedReportScreenRect
                    {
                        Rect = new SerializedReportRect
                        {
                            Top = 10,
                            Left = 10,
                            Bottom = 20,
                            Right = 20,
                        },
                    },
                },
            },
            ScreenElements = new SerializedReportScreenElements(),
        };
        
        HttpResponseMessage response = client.PostAsync("/lbp/grief", new StringContent(report.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));

        context.Database.Refresh();

        DatabaseList<GameGriefReport> reports = context.Database.GetReportsByReporter(user, 10, 0);
        Assert.That(reports.TotalItems, Is.EqualTo(1));
    
        GameGriefReport createdReport = reports.Items.First();
        Assert.That(createdReport.Level?.LevelId, Is.EqualTo(level.LevelId));
        Assert.That(createdReport.Type, Is.EqualTo(GriefReportType.TermsOfService));
        Assert.That(createdReport.Description, Is.EqualTo("This is a description, sent by LBP3"));
        Assert.That(createdReport.PhotoAssetHash, Is.EqualTo(TEST_ASSET_HASH));
        Assert.That(createdReport.Status, Is.EqualTo(GriefReportStatus.Pending));
        Assert.That(createdReport.Players.Count, Is.EqualTo(1));
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

        SerializedGriefReport report = new()
        {
            LevelId = int.MaxValue,
            JpegHash = TEST_ASSET_HASH,
        };
        
        HttpResponseMessage response = client.PostAsync("/lbp/grief", new StringContent(report.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));
        
        context.Database.Refresh();

        DatabaseList<GameGriefReport> reports = context.Database.GetReportsByReporter(user, 10, 0);
        Assert.That(reports.TotalItems, Is.EqualTo(1));
        
        Assert.That(reports.Items.First().Level, Is.Null);
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
        
        SerializedGriefReport report = new()
        {
            LevelId = 0,
            JpegHash = TEST_ASSET_HASH,
            Players = new SerializedReportPlayer[]
            {
                new()
                {
                    Username = "hee",
                    Rectangle = new SerializedReportRect(),
                },
                new()
                {
                    Username = "hawk",
                    Rectangle = new SerializedReportRect(),
                },
                new()
                {
                    Username = "2uh",
                    Rectangle = new SerializedReportRect(),
                },
                new()
                {
                    Username = "burger",
                    Rectangle = new SerializedReportRect(),
                },
                new()
                {
                    Username = "er",
                    Rectangle = new SerializedReportRect(),
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
        
        SerializedGriefReport report = new()
        {
            LevelId = 0,
            JpegHash = TEST_ASSET_HASH,
            ScreenElements = new SerializedReportScreenElements
            {
                Player = new SerializedReportPlayer[]
                {
                    new()
                    {
                        Username = "hee",
                        Rectangle = new SerializedReportRect(),
                    },
                    new()
                    {
                        Username = "haw",
                        Rectangle = new SerializedReportRect(),
                    },
                    new()
                    {
                        Username = "ham",
                        Rectangle = new SerializedReportRect(),
                    },
                    new()
                    {
                        Username = "burg",
                        Rectangle = new SerializedReportRect(),
                    },
                    new()
                    {
                        Username = "er",
                        Rectangle = new SerializedReportRect(),
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
    [TestCase(TokenGame.BetaBuild)]
    public void RedirectGriefReportToPhoto(TokenGame game)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        user.RedirectGriefReportsToPhotos = true;
        
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, game, TokenPlatform.PS3, user);

        //Upload our """photo"""
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent(TestAsset)).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        HttpResponseMessage response = client.PostAsync("/lbp/grief", new StringContent(new SerializedGriefReport
        {
            Players = new SerializedReportPlayer[]
            {
                new()
                {
                    Username = user.Username,
                    Rectangle = new SerializedReportRect
                    {
                        Top = 10,
                        Left = 10,
                        Bottom = 20,
                        Right = 20,
                    },
                },
            },
            LevelId = level.LevelId,
            LevelType = "user",
            JpegHash = TEST_ASSET_HASH,
        }.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));
        
        context.Database.Refresh();

        DatabaseList<GamePhoto> photos = context.Database.GetPhotosByUser(user, 10, 0);
        Assert.That(photos.TotalItems, Is.EqualTo(1));

        GamePhoto photo = photos.Items.First();
        Assert.That(photo.Publisher.UserId, Is.EqualTo(user.UserId));

        Assert.That(photo.OriginalLevelId, Is.EqualTo(level.LevelId));
        Assert.That(photo.LevelId, Is.EqualTo(level.LevelId));
        Assert.That(photo.LevelType, Is.EqualTo("user"));
        Assert.That(photo.OriginalLevelName, Is.EqualTo(level.Title));
    }
    
    [TestCase(TokenGame.LittleBigPlanet1)]
    [TestCase(TokenGame.LittleBigPlanet2)]
    [TestCase(TokenGame.LittleBigPlanet3)]
    [TestCase(TokenGame.LittleBigPlanetVita)]
    [TestCase(TokenGame.LittleBigPlanetPSP)]
    [TestCase(TokenGame.BetaBuild)]
    public void RedirectGriefReportToPhotoDeveloperLevel(TokenGame game)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        user.RedirectGriefReportsToPhotos = true;
        GameLevel level = context.Database.GetStoryLevelById(100000);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, game, TokenPlatform.PS3, user);

        //Upload our """photo"""
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent(TestAsset)).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        HttpResponseMessage response = client.PostAsync("/lbp/grief", new StringContent(new SerializedGriefReport
        {
            Players = new SerializedReportPlayer[]
            {
                new()
                {
                    Username = user.Username,
                    Rectangle = new SerializedReportRect
                    {
                        Top = 10,
                        Left = 10,
                        Bottom = 20,
                        Right = 20,
                    },
                },
            },
            LevelId = level.StoryId,
            LevelType = "developer",
            JpegHash = TEST_ASSET_HASH,
        }.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));
        
        context.Database.Refresh();

        DatabaseList<GamePhoto> photos = context.Database.GetPhotosByUser(user, 10, 0);
        Assert.That(photos.TotalItems, Is.EqualTo(1));

        GamePhoto photo = photos.Items.First();
        Assert.That(photo.Publisher.UserId, Is.EqualTo(user.UserId));

        Assert.That(photo.OriginalLevelId, Is.EqualTo(level.StoryId));
        Assert.That(photo.LevelId, Is.EqualTo(level.LevelId));
        Assert.That(photo.LevelType, Is.EqualTo("developer"));
        Assert.That(photo.OriginalLevelName, Is.EqualTo(level.Title));
    }

    [TestCase(TokenGame.LittleBigPlanet1)]
    [TestCase(TokenGame.LittleBigPlanet2)]
    [TestCase(TokenGame.LittleBigPlanet3)]
    [TestCase(TokenGame.LittleBigPlanetVita)]
    [TestCase(TokenGame.LittleBigPlanetPSP)]
    [TestCase(TokenGame.BetaBuild)]
    public void RedirectGriefReportToPhotoWhenNotUserOrDeveloperSlot(TokenGame game)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        user.RedirectGriefReportsToPhotos = true;

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, game, TokenPlatform.PS3, user);

        //Upload our """photo"""
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent(TestAsset)).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        HttpResponseMessage response = client.PostAsync("/lbp/grief", new StringContent(new SerializedGriefReport
        {
            Players = new SerializedReportPlayer[]
            {
                new()
                {
                    Username = user.Username,
                    Rectangle = new SerializedReportRect
                    {
                        Top = 10,
                        Left = 10,
                        Bottom = 20,
                        Right = 20,
                    },
                },
            },
            LevelId = 0,
            LevelType = "pod",
            JpegHash = TEST_ASSET_HASH,
        }.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));
        
        context.Database.Refresh();

        DatabaseList<GamePhoto> photos = context.Database.GetPhotosByUser(user, 10, 0);
        Assert.That(photos.TotalItems, Is.EqualTo(1));

        GamePhoto photo = photos.Items.First();
        Assert.That(photo.Publisher.UserId, Is.EqualTo(user.UserId));

        Assert.That(photo.OriginalLevelId, Is.EqualTo(0));
        Assert.That(photo.LevelId, Is.Null);
        Assert.That(photo.LevelType, Is.EqualTo("pod"));
        Assert.That(photo.OriginalLevelName, Is.Empty);
    }

    [TestCase(TokenGame.LittleBigPlanet1)]
    [TestCase(TokenGame.LittleBigPlanet2)]
    [TestCase(TokenGame.LittleBigPlanet3)]
    [TestCase(TokenGame.LittleBigPlanetVita)]
    [TestCase(TokenGame.LittleBigPlanetPSP)]
    [TestCase(TokenGame.BetaBuild)]
    public void RedirectGriefReportToPhotoWhenMissingLevel(TokenGame game)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        user.RedirectGriefReportsToPhotos = true;

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, game, TokenPlatform.PS3, user);

        //Upload our """photo"""
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent(TestAsset)).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        HttpResponseMessage response = client.PostAsync("/lbp/grief", new StringContent(new SerializedGriefReport
        {
            Players = new SerializedReportPlayer[]
            {
                new()
                {
                    Username = user.Username,
                    Rectangle = new SerializedReportRect
                    {
                        Top = 10,
                        Left = 10,
                        Bottom = 20,
                        Right = 20,
                    },
                },
            },
            LevelId = 1,
            LevelType = "user",
            JpegHash = TEST_ASSET_HASH,
        }.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));
        
        context.Database.Refresh();

        DatabaseList<GamePhoto> photos = context.Database.GetPhotosByUser(user, 10, 0);
        Assert.That(photos.TotalItems, Is.EqualTo(1));

        GamePhoto photo = photos.Items.First();
        Assert.That(photo.Publisher.UserId, Is.EqualTo(user.UserId));

        Assert.That(photo.OriginalLevelId, Is.EqualTo(1));
        Assert.That(photo.LevelId, Is.Null);
        Assert.That(photo.LevelType, Is.EqualTo("user"));
        Assert.That(photo.OriginalLevelName, Is.Empty);
    }
    
    [Test]
    public void CantRedirectGriefReportToPhotoWhenWebsiteToken()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        user.RedirectGriefReportsToPhotos = true;

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.Website, TokenPlatform.PS3, user);

        HttpResponseMessage response = client.PostAsync("/lbp/grief", new StringContent(new SerializedGriefReport
        {
            Players = new SerializedReportPlayer[]
            {
                new()
                {
                    Username = user.Username,
                    Rectangle = new SerializedReportRect
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

    [Test]
    [TestCase(-200, -170, -50, -1)] // negatives
    [TestCase(0, 1, 2, 3)] // what the game normally sends
    [TestCase(1, 2, 4, 8)] // too large
    [TestCase(1, 2, 2, 4)] // conflicting
    public void DontKeepOriginalPlayerNumbersOnReport(int player1Num, int player2Num, int player3Num, int player4Num)
    {
        using TestContext context = this.GetServer();
        GameUser player1 = context.CreateUser();
        GameUser player2 = context.CreateUser();
        GameUser player3 = context.CreateUser();
        GameUser player4 = context.CreateUser();
        GameLevel level = context.CreateLevel(player1);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, player1);
        
        //Upload our """photo"""
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent(TestAsset)).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedGriefReport report = new()
        {
            InfoBubble = [],
            Type = GriefReportType.TermsOfService,
            Marqee = new SerializedReportMarqee
            {
                Rect = new SerializedReportRect
                {
                    Top = 10,
                    Left = 10,
                    Bottom = 20,
                    Right = 20,
                },
            },
            LevelOwner = level.Publisher!.Username,
            LevelType = "user",
            LevelId = level.LevelId,
            JpegHash = TEST_ASSET_HASH,
            Players = new SerializedReportPlayer[]
            {
                new()
                {
                    PlayerNumber = player1Num,
                    Username = player1.Username,
                    Rectangle = new SerializedReportRect
                    {
                        Top = 10,
                        Left = 10,
                        Bottom = 20,
                        Right = 20,
                    },
                    Reporter = true,
                    IngameNow = true,
                    Text = "Some text",
                    ScreenRect = new SerializedReportScreenRect
                    {
                        Rect = new SerializedReportRect
                        {
                            Top = 10,
                            Left = 10,
                            Bottom = 20,
                            Right = 20,
                        },
                    },
                },
                new()
                {
                    PlayerNumber = 0,
                    Username = player2.Username,
                    Rectangle = new SerializedReportRect
                    {
                        Top = 10,
                        Left = 10,
                        Bottom = 20,
                        Right = 20,
                    },
                    Reporter = true,
                    IngameNow = true,
                    Text = "Some text",
                    ScreenRect = new SerializedReportScreenRect
                    {
                        Rect = new SerializedReportRect
                        {
                            Top = 10,
                            Left = 10,
                            Bottom = 20,
                            Right = 20,
                        },
                    },
                },
                new()
                {
                    PlayerNumber = 1,
                    Username = player3.Username,
                    Rectangle = new SerializedReportRect
                    {
                        Top = 10,
                        Left = 10,
                        Bottom = 20,
                        Right = 20,
                    },
                    Reporter = true,
                    IngameNow = true,
                    Text = "Some text",
                    ScreenRect = new SerializedReportScreenRect
                    {
                        Rect = new SerializedReportRect
                        {
                            Top = 10,
                            Left = 10,
                            Bottom = 20,
                            Right = 20,
                        },
                    },
                },
                new()
                {
                    PlayerNumber = 6,
                    Username = player4.Username,
                    Rectangle = new SerializedReportRect
                    {
                        Top = 10,
                        Left = 10,
                        Bottom = 20,
                        Right = 20,
                    },
                    Reporter = true,
                    IngameNow = true,
                    Text = "Some text",
                    ScreenRect = new SerializedReportScreenRect
                    {
                        Rect = new SerializedReportRect
                        {
                            Top = 10,
                            Left = 10,
                            Bottom = 20,
                            Right = 20,
                        },
                    },
                },
            },
            ScreenElements = new SerializedReportScreenElements(),
        };
        
        HttpResponseMessage response = client.PostAsync("/lbp/grief", new StringContent(report.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));

        context.Database.Refresh();

        DatabaseList<GameGriefReport> reports = context.Database.GetReportsByReporter(player1, 10, 0);
        Assert.That(reports.TotalItems, Is.EqualTo(1));
    
        GameGriefReport createdReport = reports.Items.First();
        Assert.That(createdReport.Players.Count, Is.EqualTo(4));

        ReportPlayerRelation? relation1 = createdReport.Players.FirstOrDefault(p => p.UserId == player1.UserId);
        Assert.That(relation1, Is.Not.Null);
        Assert.That(relation1.Username, Is.EqualTo(player1.Username)); // username still matches aswell
        Assert.That(relation1.PlayerNumber, Is.EqualTo(1)); // was corrected

        ReportPlayerRelation? relation2 = createdReport.Players.FirstOrDefault(p => p.UserId == player2.UserId);
        Assert.That(relation2, Is.Not.Null);
        Assert.That(relation2.Username, Is.EqualTo(player2.Username)); // username still matches aswell
        Assert.That(relation2.PlayerNumber, Is.EqualTo(2)); // was corrected

        ReportPlayerRelation? relation3 = createdReport.Players.FirstOrDefault(p => p.UserId == player3.UserId);
        Assert.That(relation3, Is.Not.Null);
        Assert.That(relation3.Username, Is.EqualTo(player3.Username)); // username still matches aswell
        Assert.That(relation3.PlayerNumber, Is.EqualTo(3)); // was corrected

        ReportPlayerRelation? relation4 = createdReport.Players.FirstOrDefault(p => p.UserId == player4.UserId);
        Assert.That(relation4, Is.Not.Null);
        Assert.That(relation4.Username, Is.EqualTo(player4.Username)); // username still matches aswell
        Assert.That(relation4.PlayerNumber, Is.EqualTo(4)); // was corrected
    }

    [Test]
    public void UploadReportWithUnknownUsers()
    {
        using TestContext context = this.GetServer();
        GameUser existentUser = context.CreateUser("negative");
        GameLevel level = context.CreateLevel(existentUser);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, existentUser);
        
        //Upload our """photo"""
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent(TestAsset)).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedGriefReport report = new()
        {
            InfoBubble = [],
            Type = GriefReportType.TermsOfService,
            Marqee = new SerializedReportMarqee
            {
                Rect = new SerializedReportRect
                {
                    Top = 10,
                    Left = 10,
                    Bottom = 20,
                    Right = 20,
                },
            },
            LevelOwner = level.Publisher!.Username,
            LevelType = "user",
            LevelId = level.LevelId,
            JpegHash = TEST_ASSET_HASH,
            Players = new SerializedReportPlayer[]
            {
                new()
                {
                    PlayerNumber = 0,
                    Username = existentUser.Username,
                    Rectangle = new SerializedReportRect
                    {
                        Top = 10,
                        Left = 10,
                        Bottom = 20,
                        Right = 20,
                    },
                    Reporter = true,
                    IngameNow = true,
                    Text = "Some text",
                    ScreenRect = new SerializedReportScreenRect
                    {
                        Rect = new SerializedReportRect
                        {
                            Top = 10,
                            Left = 10,
                            Bottom = 20,
                            Right = 20,
                        },
                    },
                },
                new()
                {
                    PlayerNumber = 0,
                    Username = "fromBacon",
                    Rectangle = new SerializedReportRect
                    {
                        Top = 10,
                        Left = 10,
                        Bottom = 20,
                        Right = 20,
                    },
                    Reporter = true,
                    IngameNow = true,
                    Text = "Some text",
                    ScreenRect = new SerializedReportScreenRect
                    {
                        Rect = new SerializedReportRect
                        {
                            Top = 10,
                            Left = 10,
                            Bottom = 20,
                            Right = 20,
                        },
                    },
                },
                new()
                {
                    PlayerNumber = 1,
                    Username = "localGuest",
                    Rectangle = new SerializedReportRect
                    {
                        Top = 10,
                        Left = 10,
                        Bottom = 20,
                        Right = 20,
                    },
                    Reporter = true,
                    IngameNow = true,
                    Text = "Some text",
                    ScreenRect = new SerializedReportScreenRect
                    {
                        Rect = new SerializedReportRect
                        {
                            Top = 10,
                            Left = 10,
                            Bottom = 20,
                            Right = 20,
                        },
                    },
                },
                new()
                {
                    PlayerNumber = 1,
                    Username = "Xx_Marcell_Davis_xX",
                    Rectangle = new SerializedReportRect
                    {
                        Top = 10,
                        Left = 10,
                        Bottom = 20,
                        Right = 20,
                    },
                    Reporter = true,
                    IngameNow = true,
                    Text = "Some text",
                    ScreenRect = new SerializedReportScreenRect
                    {
                        Rect = new SerializedReportRect
                        {
                            Top = 10,
                            Left = 10,
                            Bottom = 20,
                            Right = 20,
                        },
                    },
                },
            },
            ScreenElements = new SerializedReportScreenElements(),
        };
        
        HttpResponseMessage response = client.PostAsync("/lbp/grief", new StringContent(report.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));

        context.Database.Refresh();

        DatabaseList<GameGriefReport> reports = context.Database.GetReportsByReporter(existentUser, 10, 0);
        Assert.That(reports.TotalItems, Is.EqualTo(1));
    
        GameGriefReport createdReport = reports.Items.First();
        Assert.That(createdReport.Players.Count, Is.EqualTo(4));

        // relation exists with correct number, username, and the correct user ID (and included user)
        ReportPlayerRelation? relation1 = createdReport.Players.FirstOrDefault(p => p.Username == existentUser.Username);
        Assert.That(relation1, Is.Not.Null);
        Assert.That(relation1.PlayerNumber, Is.EqualTo(1));
        Assert.That(relation1.UserId, Is.EqualTo(existentUser.UserId));
        Assert.That(relation1.User, Is.Not.Null);

        // relation exists with correct number and username, but no user ID (and no included user)
        ReportPlayerRelation? relation2 = createdReport.Players.FirstOrDefault(p => p.Username == "fromBacon");
        Assert.That(relation2, Is.Not.Null);
        Assert.That(relation1.PlayerNumber, Is.EqualTo(2));
        Assert.That(relation2.UserId, Is.Null);
        Assert.That(relation2.User, Is.Null);

        // relation exists with correct number and username, but no user ID (and no included user)
        ReportPlayerRelation? relation3 = createdReport.Players.FirstOrDefault(p => p.Username == "localGuest");
        Assert.That(relation3, Is.Not.Null);
        Assert.That(relation1.PlayerNumber, Is.EqualTo(3));
        Assert.That(relation3.UserId, Is.Null);
        Assert.That(relation3.User, Is.Null);

        // relation exists with correct number and username, but no user ID (and no included user)
        ReportPlayerRelation? relation4 = createdReport.Players.FirstOrDefault(p => p.Username == "Xx_Marcell_Davis_xX");
        Assert.That(relation4, Is.Not.Null);
        Assert.That(relation1.PlayerNumber, Is.EqualTo(4));
        Assert.That(relation4.UserId, Is.Null);
        Assert.That(relation4.User, Is.Null);
    }
}