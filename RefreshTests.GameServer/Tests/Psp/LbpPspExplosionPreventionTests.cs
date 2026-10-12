using System.Reflection;
using Refresh.Common.Helpers;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Reports;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Types.Report;
using Refresh.Interfaces.Game.Types.UserData.Leaderboard;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Psp;

public class LbpPspExplosionPreventionTests : GameServerTest
{
    private const string TEST_ASSET_HASH = "0ec63b140374ba704a58fa0c743cb357683313dd";
    private static readonly byte[] TestAsset = ResourceHelper.ReadResource("RefreshTests.GameServer.Resources.1x1.png", Assembly.GetExecutingAssembly());

    [Test]
    public void GetOkOnPlayErrorsIfPsp()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(context.CreateUser());

        HttpClient nonPsp = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet2, TokenPlatform.PS3, user);
        HttpClient psp = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanetPSP, TokenPlatform.PSP, user);
        psp.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");
        context.Database.Refresh();

        // Ensure both get OK if everything is OK
        Assert.That(nonPsp.PostAsync($"/lbp/play/user/{level.LevelId}", null).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(psp.PostAsync($"/lbp/play/user/{level.LevelId}", null).Result.StatusCode, Is.EqualTo(OK));

        // Non-existent level
        Assert.That(nonPsp.PostAsync($"/lbp/play/user/22432", null).Result.StatusCode, Is.EqualTo(NotFound));
        Assert.That(psp.PostAsync($"/lbp/play/user/22432", null).Result.StatusCode, Is.EqualTo(OK));
    }

    [Test]
    public void GetOkOnLevelHeartErrorsIfPsp()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(context.CreateUser());

        HttpClient nonPsp = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet2, TokenPlatform.PS3, user);
        HttpClient psp = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanetPSP, TokenPlatform.PSP, user);
        psp.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");
        
        // Ensure both get OK if everything is OK
        Assert.That(nonPsp.PostAsync($"/lbp/favourite/slot/user/{level.LevelId}", null).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(psp.PostAsync($"/lbp/favourite/slot/user/{level.LevelId}", null).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(nonPsp.PostAsync($"/lbp/unfavourite/slot/user/{level.LevelId}", null).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(psp.PostAsync($"/lbp/unfavourite/slot/user/{level.LevelId}", null).Result.StatusCode, Is.EqualTo(OK));

        // Non-existent level
        Assert.That(nonPsp.PostAsync($"/lbp/favourite/slot/user/22432", null).Result.StatusCode, Is.EqualTo(NotFound));
        Assert.That(psp.PostAsync($"/lbp/favourite/slot/user/22432", null).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(nonPsp.PostAsync($"/lbp/unfavourite/slot/user/22432", null).Result.StatusCode, Is.EqualTo(NotFound));
        Assert.That(psp.PostAsync($"/lbp/unfavourite/slot/user/22432", null).Result.StatusCode, Is.EqualTo(OK));

        // Blocked by read-only mode
        context.Server.Value.GameServerConfig.NewUserPermissions.ReadOnlyMode = true;
        context.Server.Value.GameServerConfig.NormalUserPermissions.ReadOnlyMode = true;
        context.Server.Value.GameServerConfig.TrustedUserPermissions.ReadOnlyMode = true;
        
        Assert.That(nonPsp.PostAsync($"/lbp/favourite/slot/user/{level.LevelId}", null).Result.StatusCode, Is.EqualTo(Unauthorized));
        Assert.That(psp.PostAsync($"/lbp/favourite/slot/user/{level.LevelId}", null).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(nonPsp.PostAsync($"/lbp/unfavourite/slot/user/{level.LevelId}", null).Result.StatusCode, Is.EqualTo(Unauthorized));
        Assert.That(psp.PostAsync($"/lbp/unfavourite/slot/user/{level.LevelId}", null).Result.StatusCode, Is.EqualTo(OK));
    }

    [Test]
    public void GetOkOnUserHeartErrorsIfPsp()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameUser userToHeart = context.CreateUser();

        HttpClient nonPsp = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet2, TokenPlatform.PS3, user);
        HttpClient psp = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanetPSP, TokenPlatform.PSP, user);
        psp.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");
        
        // Ensure both get OK if everything is OK
        Assert.That(nonPsp.PostAsync($"/lbp/favourite/user/{userToHeart.Username}", null).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(psp.PostAsync($"/lbp/favourite/user/{userToHeart.Username}", null).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(nonPsp.PostAsync($"/lbp/unfavourite/user/{userToHeart.Username}", null).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(psp.PostAsync($"/lbp/unfavourite/user/{userToHeart.Username}", null).Result.StatusCode, Is.EqualTo(OK));

        // Non-existent user
        Assert.That(nonPsp.PostAsync($"/lbp/favourite/user/lel420", null).Result.StatusCode, Is.EqualTo(NotFound));
        Assert.That(psp.PostAsync($"/lbp/favourite/user/lel420", null).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(nonPsp.PostAsync($"/lbp/unfavourite/user/lel420", null).Result.StatusCode, Is.EqualTo(NotFound));
        Assert.That(psp.PostAsync($"/lbp/unfavourite/user/lel420", null).Result.StatusCode, Is.EqualTo(OK));

        // Blocked by read-only mode
        context.Server.Value.GameServerConfig.NewUserPermissions.ReadOnlyMode = true;
        context.Server.Value.GameServerConfig.NormalUserPermissions.ReadOnlyMode = true;
        context.Server.Value.GameServerConfig.TrustedUserPermissions.ReadOnlyMode = true;
        
        Assert.That(nonPsp.PostAsync($"/lbp/favourite/user/{userToHeart.Username}", null).Result.StatusCode, Is.EqualTo(Unauthorized));
        Assert.That(psp.PostAsync($"/lbp/favourite/user/{userToHeart.Username}", null).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(nonPsp.PostAsync($"/lbp/unfavourite/user/{userToHeart.Username}", null).Result.StatusCode, Is.EqualTo(Unauthorized));
        Assert.That(psp.PostAsync($"/lbp/unfavourite/user/{userToHeart.Username}", null).Result.StatusCode, Is.EqualTo(OK));
    }

    [Test]
    public void GetOkOnLevelRateErrorsIfPsp()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(context.CreateUser());

        HttpClient nonPsp = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, user);
        HttpClient psp = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanetPSP, TokenPlatform.PSP, user);
        psp.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");

        // Ensure both get OK if everything is OK
        Assert.That(nonPsp.PostAsync($"/lbp/rate/slot/user/{level.LevelId}?rating=1", null).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(psp.PostAsync($"/lbp/rate/slot/user/{level.LevelId}?rating=1", null).Result.StatusCode, Is.EqualTo(OK));

        // Non-existent level
        Assert.That(nonPsp.PostAsync($"/lbp/rate/slot/user/22432?rating=1", null).Result.StatusCode, Is.EqualTo(NotFound));
        Assert.That(psp.PostAsync($"/lbp/rate/slot/user/22432?rating=1", null).Result.StatusCode, Is.EqualTo(OK));

        // Invalid rating
        Assert.That(nonPsp.PostAsync($"/lbp/rate/slot/user/{level.LevelId}?rating=67", null).Result.StatusCode, Is.EqualTo(BadRequest));
        Assert.That(psp.PostAsync($"/lbp/rate/slot/user/{level.LevelId}?rating=67", null).Result.StatusCode, Is.EqualTo(OK));

        // Missing rating
        Assert.That(nonPsp.PostAsync($"/lbp/rate/slot/user/{level.LevelId}", null).Result.StatusCode, Is.EqualTo(BadRequest));
        Assert.That(psp.PostAsync($"/lbp/rate/slot/user/{level.LevelId}", null).Result.StatusCode, Is.EqualTo(OK));

        // Blocked by read-only mode
        context.Server.Value.GameServerConfig.NewUserPermissions.ReadOnlyMode = true;
        context.Server.Value.GameServerConfig.NormalUserPermissions.ReadOnlyMode = true;
        context.Server.Value.GameServerConfig.TrustedUserPermissions.ReadOnlyMode = true;
        
        Assert.That(nonPsp.PostAsync($"/lbp/rate/slot/user/{level.LevelId}?rating=1", null).Result.StatusCode, Is.EqualTo(Unauthorized));
        Assert.That(psp.PostAsync($"/lbp/rate/slot/user/{level.LevelId}?rating=1", null).Result.StatusCode, Is.EqualTo(OK));
    }

    [Test]
    public void GetOkOnLevelTagErrorsIfPsp()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(context.CreateUser());

        HttpClient nonPsp = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, user);
        HttpClient psp = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanetPSP, TokenPlatform.PSP, user);
        psp.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");

        // Ensure both get OK if everything is OK
        Assert.That(nonPsp.PostAsync($"/lbp/tag/user/{level.LevelId}", new StringContent("t=TAG_Weird")).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(psp.PostAsync($"/lbp/tag/user/{level.LevelId}", new StringContent("t=TAG_Weird")).Result.StatusCode, Is.EqualTo(OK));

        // Non-existent level
        Assert.That(nonPsp.PostAsync($"/lbp/tag/user/22432", new StringContent("t=TAG_Weird")).Result.StatusCode, Is.EqualTo(NotFound));
        Assert.That(psp.PostAsync($"/lbp/tag/user/22432", new StringContent("t=TAG_Weird")).Result.StatusCode, Is.EqualTo(OK));

        // Invalid tag
        Assert.That(nonPsp.PostAsync($"/lbp/tag/user/{level.LevelId}", new StringContent("t=TAG_ILOVEPSP")).Result.StatusCode, Is.EqualTo(BadRequest));
        Assert.That(psp.PostAsync($"/lbp/tag/user/{level.LevelId}", new StringContent("t=TAG_ILOVEPSP")).Result.StatusCode, Is.EqualTo(OK));

        // Missing tag
        Assert.That(nonPsp.PostAsync($"/lbp/tag/user/{level.LevelId}", new StringContent("t=")).Result.StatusCode, Is.EqualTo(BadRequest));
        Assert.That(psp.PostAsync($"/lbp/tag/user/{level.LevelId}", new StringContent("t=")).Result.StatusCode, Is.EqualTo(OK));

        // Missing body
        Assert.That(nonPsp.PostAsync($"/lbp/tag/user/{level.LevelId}", null).Result.StatusCode, Is.EqualTo(BadRequest));
        Assert.That(psp.PostAsync($"/lbp/tag/user/{level.LevelId}", null).Result.StatusCode, Is.EqualTo(OK));

        // Blocked by read-only mode
        context.Server.Value.GameServerConfig.NewUserPermissions.ReadOnlyMode = true;
        context.Server.Value.GameServerConfig.NormalUserPermissions.ReadOnlyMode = true;
        context.Server.Value.GameServerConfig.TrustedUserPermissions.ReadOnlyMode = true;
        
        Assert.That(nonPsp.PostAsync($"/lbp/tag/user/{level.LevelId}?t=TAG_Weird", null).Result.StatusCode, Is.EqualTo(Unauthorized));
        Assert.That(psp.PostAsync($"/lbp/tag/user/{level.LevelId}?t=TAG_Weird", null).Result.StatusCode, Is.EqualTo(OK));
    }

    [Test]
    public void GetOkOnScoreUploadErrorsIfPsp()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(context.CreateUser());

        HttpClient nonPsp = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, user);
        HttpClient psp = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanetPSP, TokenPlatform.PSP, user);
        psp.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");

        SerializedScore score = new()
        {
            Host = true,
            ScoreType = 1,
            Score = 5,
            PlayerUsernames = [user.Username],
        };

        // Didn't play the level yet (PSP score is still saved)
        Assert.That(nonPsp.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result.StatusCode, Is.EqualTo(Unauthorized));
        Assert.That(psp.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(context.Database.GetTotalCompletionsForLevelByUser(level, user), Is.EqualTo(1));

        // Ensure both get OK if the level is played, and everything else is OK
        HttpResponseMessage message = nonPsp.PostAsync($"/lbp/play/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        Assert.That(nonPsp.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(psp.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(context.Database.GetTotalCompletionsForLevelByUser(level, user), Is.EqualTo(3));

        // Non-existent level
        Assert.That(nonPsp.PostAsync($"/lbp/scoreboard/user/67676", new StringContent(score.AsXML())).Result.StatusCode, Is.EqualTo(NotFound));
        Assert.That(psp.PostAsync($"/lbp/scoreboard/user/67676", new StringContent(score.AsXML())).Result.StatusCode, Is.EqualTo(OK));

        // Invalid XML in response
        Assert.That(nonPsp.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent("nut")).Result.StatusCode, Is.EqualTo(BadRequest));
        Assert.That(psp.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent("nut")).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(context.Database.GetTotalCompletionsForLevelByUser(level, user), Is.EqualTo(3));

        // Score is negative (PSP score is still saved)
        score.Score = -420;
        Assert.That(nonPsp.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result.StatusCode, Is.EqualTo(BadRequest));
        Assert.That(psp.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(context.Database.GetTotalCompletionsForLevelByUser(level, user), Is.EqualTo(4));

        // Score is too high (PSP score is still saved)
        score.Score = int.MaxValue;
        Assert.That(nonPsp.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result.StatusCode, Is.EqualTo(BadRequest));
        Assert.That(psp.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(context.Database.GetTotalCompletionsForLevelByUser(level, user), Is.EqualTo(5));

        // Missing username in participant list (PSP score is still saved)
        score.Score = 5;
        score.PlayerUsernames = [];
        Assert.That(nonPsp.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result.StatusCode, Is.EqualTo(BadRequest));
        Assert.That(psp.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(context.Database.GetTotalCompletionsForLevelByUser(level, user), Is.EqualTo(6));

        // Invalid score type (PSP score is still saved)
        score.ScoreType = 90;
        score.PlayerUsernames = [];
        Assert.That(nonPsp.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result.StatusCode, Is.EqualTo(BadRequest));
        Assert.That(psp.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(context.Database.GetTotalCompletionsForLevelByUser(level, user), Is.EqualTo(7));

        // Blocked by read-only mode
        context.Server.Value.GameServerConfig.NewUserPermissions.ReadOnlyMode = true;
        context.Server.Value.GameServerConfig.NormalUserPermissions.ReadOnlyMode = true;
        context.Server.Value.GameServerConfig.TrustedUserPermissions.ReadOnlyMode = true;
        
        Assert.That(nonPsp.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result.StatusCode, Is.EqualTo(Unauthorized));
        Assert.That(psp.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(context.Database.GetTotalCompletionsForLevelByUser(level, user), Is.EqualTo(7));
    }

    [Test]
    public void GetOkOnGriefReportErrorsIfPsp()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(context.CreateUser());

        HttpClient nonPsp = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, user);
        HttpClient psp = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanetPSP, TokenPlatform.PSP, user);
        psp.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");

        //Upload our """photo"""
        HttpResponseMessage message = nonPsp.PostAsync($"/lbp/upload/{TEST_ASSET_HASH}", new ReadOnlyMemoryContent(TestAsset)).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        GameReport report = new()
        {
            InfoBubble = [],
            Type = GriefReportType.TermsOfService,
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
            Players =
            [
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
            ],
            ScreenElements = new ScreenElements(),
        };

        // Ensure both get OK if everything is OK
        Assert.That(nonPsp.PostAsync("/lbp/grief", new StringContent(report.AsXML())).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(psp.PostAsync("/lbp/grief", new StringContent(report.AsXML())).Result.StatusCode, Is.EqualTo(OK));

        // Insert too many players
        for (int i = 0; i < 7; i++)
        {
            report.Players = report.Players.Append(new()
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
                }
            ).ToArray();
        }

        Assert.That(nonPsp.PostAsync("/lbp/grief", new StringContent(report.AsXML())).Result.StatusCode, Is.EqualTo(BadRequest));
        Assert.That(psp.PostAsync("/lbp/grief", new StringContent(report.AsXML())).Result.StatusCode, Is.EqualTo(OK));

        // Invalid XML in request
        Assert.That(nonPsp.PostAsync("/lbp/grief", new StringContent("these")).Result.StatusCode, Is.EqualTo(BadRequest));
        Assert.That(psp.PostAsync("/lbp/grief", new StringContent("these")).Result.StatusCode, Is.EqualTo(OK));

        // Blocked by read-only mode
        context.Server.Value.GameServerConfig.NewUserPermissions.ReadOnlyMode = true;
        context.Server.Value.GameServerConfig.NormalUserPermissions.ReadOnlyMode = true;
        context.Server.Value.GameServerConfig.TrustedUserPermissions.ReadOnlyMode = true;
        
        Assert.That(nonPsp.PostAsync("/lbp/grief", new StringContent(report.AsXML())).Result.StatusCode, Is.EqualTo(Unauthorized));
        Assert.That(psp.PostAsync("/lbp/grief", new StringContent(report.AsXML())).Result.StatusCode, Is.EqualTo(OK));
    }
}