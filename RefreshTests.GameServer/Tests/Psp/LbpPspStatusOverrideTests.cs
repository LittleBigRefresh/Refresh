using System.Reflection;
using Refresh.Common.Helpers;
using Refresh.Core.Configuration;
using Refresh.Core.RateLimits.EndpointRateLimiting;
using Refresh.Core.Services;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Reports;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Types.Report;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Psp;

public class LbpPspStatusOverrideTests : GameServerTest
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

        // Blocked by rate-limit
        EndpointRateLimitConfig rateLimitConfig = new();
        rateLimitConfig.Buckets.Add(EndpointBucketId.GamePlayLevel.ToString(), new(100, 1, 200));
        TestEndpointRateLimiter rateLimiter = new(context.Time, context.Server.Value.Logger, new());
        context.Server.Value.Server.AddService(new GameRateLimitService(context.Server.Value.Logger, rateLimiter, context.Server.Value.GetService<GameAuthenticationService>()));

        // Rate-limited on second one
        Assert.That(nonPsp.PostAsync($"/lbp/play/user/{level.LevelId}", null).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(nonPsp.PostAsync($"/lbp/play/user/{level.LevelId}", null).Result.StatusCode, Is.EqualTo(TooManyRequests));
        // still OK (user should already get rate-limited on first one, but they still receive OK)
        Assert.That(psp.PostAsync($"/lbp/play/user/{level.LevelId}", null).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(psp.PostAsync($"/lbp/play/user/{level.LevelId}", null).Result.StatusCode, Is.EqualTo(OK));
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
        Assert.That(nonPsp.PostAsync($"/lbp/rate/slot/user/22432?rating=67", null).Result.StatusCode, Is.EqualTo(BadRequest));
        Assert.That(psp.PostAsync($"/lbp/rate/slot/user/22432?rating=67", null).Result.StatusCode, Is.EqualTo(OK));

        // Missing rating
        Assert.That(nonPsp.PostAsync($"/lbp/rate/slot/user/22432", null).Result.StatusCode, Is.EqualTo(BadRequest));
        Assert.That(psp.PostAsync($"/lbp/rate/slot/user/22432", null).Result.StatusCode, Is.EqualTo(OK));

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
        Assert.That(nonPsp.PostAsync($"/lbp/tag/slot/user/{level.LevelId}?t=TAG_Weird", null).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(psp.PostAsync($"/lbp/tag/slot/user/{level.LevelId}?t=TAG_Weird", null).Result.StatusCode, Is.EqualTo(OK));

        // Non-existent level
        Assert.That(nonPsp.PostAsync($"/lbp/tag/slot/user/22432?t=TAG_Weird", null).Result.StatusCode, Is.EqualTo(NotFound));
        Assert.That(psp.PostAsync($"/lbp/tag/slot/user/22432?t=TAG_Weird", null).Result.StatusCode, Is.EqualTo(OK));

        // Invalid tag
        Assert.That(nonPsp.PostAsync($"/lbp/tag/slot/user/22432?t=TAG_ILOVEPSP", null).Result.StatusCode, Is.EqualTo(BadRequest));
        Assert.That(psp.PostAsync($"/lbp/tag/slot/user/22432?t=TAG_ILOVEPSP", null).Result.StatusCode, Is.EqualTo(OK));

        // Missing tag
        Assert.That(nonPsp.PostAsync($"/lbp/tag/slot/user/22432", null).Result.StatusCode, Is.EqualTo(BadRequest));
        Assert.That(psp.PostAsync($"/lbp/tag/slot/user/22432", null).Result.StatusCode, Is.EqualTo(OK));

        // Blocked by read-only mode
        context.Server.Value.GameServerConfig.NewUserPermissions.ReadOnlyMode = true;
        context.Server.Value.GameServerConfig.NormalUserPermissions.ReadOnlyMode = true;
        context.Server.Value.GameServerConfig.TrustedUserPermissions.ReadOnlyMode = true;
        
        Assert.That(nonPsp.PostAsync($"/lbp/tag/slot/user/{level.LevelId}?t=TAG_Weird", null).Result.StatusCode, Is.EqualTo(Unauthorized));
        Assert.That(psp.PostAsync($"/lbp/tag/slot/user/{level.LevelId}?t=TAG_Weird", null).Result.StatusCode, Is.EqualTo(OK));
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

        // Blocked by read-only mode
        context.Server.Value.GameServerConfig.NewUserPermissions.ReadOnlyMode = true;
        context.Server.Value.GameServerConfig.NormalUserPermissions.ReadOnlyMode = true;
        context.Server.Value.GameServerConfig.TrustedUserPermissions.ReadOnlyMode = true;
        
        Assert.That(nonPsp.PostAsync("/lbp/grief", new StringContent(report.AsXML())).Result.StatusCode, Is.EqualTo(Unauthorized));
        Assert.That(psp.PostAsync("/lbp/grief", new StringContent(report.AsXML())).Result.StatusCode, Is.EqualTo(OK));
    
        // Blocked by rate-limit
        EndpointRateLimitConfig rateLimitConfig = new();
        rateLimitConfig.Buckets.Add(EndpointBucketId.GameUploadGriefReport.ToString(), new(100, 1, 200));
        TestEndpointRateLimiter rateLimiter = new(context.Time, context.Server.Value.Logger, new());
        context.Server.Value.Server.AddService(new GameRateLimitService(context.Server.Value.Logger, rateLimiter, context.Server.Value.GetService<GameAuthenticationService>()));

        Assert.That(nonPsp.PostAsync("/lbp/grief", new StringContent(report.AsXML())).Result.StatusCode, Is.EqualTo(BadRequest));
        Assert.That(nonPsp.PostAsync("/lbp/grief", new StringContent(report.AsXML())).Result.StatusCode, Is.EqualTo(TooManyRequests));
        Assert.That(psp.PostAsync("/lbp/grief", new StringContent(report.AsXML())).Result.StatusCode, Is.EqualTo(OK));
        Assert.That(psp.PostAsync("/lbp/grief", new StringContent(report.AsXML())).Result.StatusCode, Is.EqualTo(OK));
    }
}