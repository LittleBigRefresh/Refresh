using Refresh.Database.Query;
using Refresh.Database.Models.Authentication;
using Refresh.Database;
using Refresh.Database.Models.Contests;
using Refresh.Database.Models.Users;
using RefreshTests.GameServer.Extensions;
using Refresh.Database.Models.Levels;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response;
using MongoDB.Bson;

namespace RefreshTests.GameServer.Tests.Levels;

public class ContestTests : GameServerTest
{
    private const string Banner = "https://i.imgur.com/n80NswV.png";
    private readonly TokenGame[] _allowedGames =
    [
        TokenGame.LittleBigPlanet1, TokenGame.LittleBigPlanet2, TokenGame.LittleBigPlanet3, TokenGame.LittleBigPlanetVita, TokenGame.LittleBigPlanetPSP,
    ];
    
    private GameContest CreateContest(TestContext context, GameUser? organizer = null, string id = "ut", TokenGame[]? allowedGames = null)
    {
        organizer ??= context.CreateUser();
        GameLevel templateLevel = context.CreateLevel(organizer);
        
        GameContest contest = context.Database.CreateContest(id, new ApiContestRequest
        {
            OrganizerId = organizer.UserId.ToString(),
            BannerUrl = Banner,
            ContestTitle = "unit test",
            ContestSummary = "summary",
            ContestTag = "#ut",
            ContestDetails = "# unit test",
            StartDate = context.Time.Now,
            EndDate = context.Time.Now + TimeSpan.FromMilliseconds(10),
            ContestTheme = "good level",
            AllowedGames = allowedGames,
        }, organizer, templateLevel);

        return contest;
    }
    
    [Test]
    public void CanCreateContest()
    {
        using TestContext context = this.GetServer(false);
        GameUser organizer = context.CreateUser();
        GameLevel templateLevel = context.CreateLevel(organizer);
        
        Assert.That(() =>
        {
            // ReSharper disable once AccessToDisposedClosure
            context.Database.CreateContest("contest", new ApiContestRequest
            {
                OrganizerId = organizer.UserId.ToString(),
                BannerUrl = Banner,
                ContestTitle = "The Contest Contest",
                ContestSummary = "a contest about contests",
                ContestTag = "#cc1",
                ContestDetails = "# test",
                StartDate = DateTimeOffset.FromUnixTimeMilliseconds(1),
                EndDate = DateTimeOffset.FromUnixTimeMilliseconds(2),
                ContestTheme = "good level",
                AllowedGames = this._allowedGames,
            }, organizer, templateLevel);
        }, Throws.Nothing);

        GameContest? contest = context.Database.GetContestById("contest");
        Assert.That(contest, Is.Not.Null);
        Assert.That(contest!.ContestTitle, Is.EqualTo("The Contest Contest"));
        Assert.That(contest.Organizer, Is.EqualTo(organizer));
    }

    [Test]
    public void CanCreateContestWithLessData()
    {
        using TestContext context = this.GetServer(false);
        GameUser organizer = context.CreateUser();
        
        Assert.That(() =>
        {
            // ReSharper disable once AccessToDisposedClosure
            context.Database.CreateContest("minicontest", new ApiContestRequest
            {
                OrganizerId = organizer.UserId.ToString(),
                ContestTitle = "The Lazy Contest",
                StartDate = DateTimeOffset.FromUnixTimeMilliseconds(1),
                EndDate = DateTimeOffset.FromUnixTimeMilliseconds(2),
            }, organizer, null);
        }, Throws.Nothing);

        GameContest? contest = context.Database.GetContestById("minicontest");
        Assert.That(contest, Is.Not.Null);
    }

    [Test]
    public void CanCreateContestFromApi()
    {
        using TestContext context = this.GetServer();
        GameUser organizer = context.CreateUser();
        GameUser admin = context.CreateUser(role: GameUserRole.Admin);
        GameLevel templateLevel = context.CreateLevel(organizer);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, admin);
        ApiResponse<ApiContestResponse>? response = client.PostData<ApiContestResponse>("/api/v3/admin/contests/cbt1", new ApiContestRequest
        {
            OrganizerId = organizer.UserId.ToString(),
            StartDate = context.Time.Now + TimeSpan.FromHours(1),
            EndDate = context.Time.Now + TimeSpan.FromHours(2),
            ContestTag = "#cbt1",
            BannerUrl = Banner,
            ContestTitle = "The You-Know-What Contest #1",
            ContestSummary = "The contest all about *that*",
            ContestDetails = "Yep Yep Yep Yep Yep",
            ContestTheme = "good level",
            AllowedGames = this._allowedGames,
            TemplateLevelId = templateLevel.LevelId,
        });
        
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Success, Is.True);

        GameContest? createdContest = context.Database.GetContestById("cbt1");
        Assert.That(createdContest, Is.Not.Null);
        Assert.That(createdContest!.Organizer, Is.EqualTo(organizer));
    }

    [Test]
    public void CannotCreateContestIfOrganizerIsInvalid()
    {
        using TestContext context = this.GetServer();
        GameUser admin = context.CreateUser(role: GameUserRole.Admin);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, admin);

        // Try uploading a contest with an invalid organizer UUID
        ApiContestRequest request = new()
        {
            OrganizerId = "hi",
            StartDate = context.Time.Now + TimeSpan.FromHours(1),
            EndDate = context.Time.Now + TimeSpan.FromHours(2),
            ContestTag = "#cbt2",
            ContestTitle = "The You-Know-What Contest #2",
            ContestSummary = "We're not done yet",
            ContestDetails = "Wikipedia",
            ContestTheme = "good level",
        };

        ApiResponse<ApiContestResponse>? response = client.PostData<ApiContestResponse>("/api/v3/admin/contests/cbt2", request, false, true); 
        Assert.That(response?.Error, Is.Not.Null);
        Assert.That(response!.Error!.StatusCode, Is.EqualTo(BadRequest));

        // Now try creating one with a valid UUID, but there is no user with this UUID
        request.OrganizerId = ObjectId.GenerateNewId().ToString();
        response = client.PostData<ApiContestResponse>("/api/v3/admin/contests/cbt3", request, false, true); 
        Assert.That(response?.Error, Is.Not.Null);
        Assert.That(response!.Error!.StatusCode, Is.EqualTo(NotFound));
    }

    [Test]
    public void CannotCreateContestIfTemplateIsInvalid()
    {
        using TestContext context = this.GetServer();
        GameUser admin = context.CreateUser(role: GameUserRole.Admin);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, admin);
        ApiResponse<ApiContestResponse>? response = client.PostData<ApiContestResponse>("/api/v3/admin/contests/oc1", new ApiContestRequest
        {
            OrganizerId = ObjectId.GenerateNewId().ToString(),
            StartDate = context.Time.Now + TimeSpan.FromHours(1),
            EndDate = context.Time.Now + TimeSpan.FromHours(2),
            ContestTag = "#oc1",
            ContestTitle = "Unoriginality Contest",
            ContestSummary = "Be unoriginal, just like this contest!",
            ContestDetails = "Submit bomb survivals",
            ContestTheme = "unoriginal level",
            TemplateLevelId = 7865432,
        }, false, true);
        
        Assert.That(response?.Error, Is.Not.Null);
        Assert.That(response!.Error!.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void CanUpdateContestFromApi()
    {
        using TestContext context = this.GetServer();
        GameUser admin = context.CreateUser(role: GameUserRole.Admin);
        GameContest contest = this.CreateContest(context);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, admin);
        ApiResponse<ApiContestResponse>? response = client.PatchData<ApiContestResponse>("/api/v3/contests/ut", new ApiContestRequest
        {
            OrganizerId = contest.Organizer.UserId.ToString(),
            ContestTag = "#ut2",
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Success, Is.True);

        context.Database.Refresh();

        GameContest? createdContest = context.Database.GetContestById("ut");
        Assert.That(createdContest, Is.Not.Null);
        Assert.That(createdContest!.ContestTag, Is.EqualTo("#ut2"));
    }

    [Test]
    public void CannotUpdateContestToInvalidOrganizer()
    {
        using TestContext context = this.GetServer();
        GameUser admin = context.CreateUser(role: GameUserRole.Admin);
        GameContest contest = this.CreateContest(context);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, admin);

        // Try setting the organizer to an invalid UUID
        ApiResponse<ApiContestResponse>? response = client.PatchData<ApiContestResponse>("/api/v3/contests/ut", new ApiContestRequest
        {
            OrganizerId = "hi",
            ContestTag = "#ut2",
        }, false, true);

        Assert.That(response?.Error, Is.Not.Null);
        Assert.That(response!.Error!.StatusCode, Is.EqualTo(BadRequest));

        // Now update it to have a valid UUID which doesn't exist
        response = client.PatchData<ApiContestResponse>("/api/v3/contests/ut", new ApiContestRequest
        {
            OrganizerId = ObjectId.GenerateNewId().ToString(),
            ContestTag = "#ut2",
        }, false, true);
        Assert.That(response?.Error, Is.Not.Null);
        Assert.That(response!.Error!.StatusCode, Is.EqualTo(NotFound));
    }

    [Test]
    public void CannotUpdateContestToInvalidTemplate()
    {
        using TestContext context = this.GetServer();
        GameUser admin = context.CreateUser(role: GameUserRole.Admin);
        GameContest contest = this.CreateContest(context);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, admin);

        // Try setting the organizer to an invalid UUID
        ApiResponse<ApiContestResponse>? response = client.PatchData<ApiContestResponse>("/api/v3/contests/ut", new ApiContestRequest
        {
            TemplateLevelId = 98765443,
            ContestTag = "#ut2",
        }, false, true);

        Assert.That(response?.Error, Is.Not.Null);
        Assert.That(response!.Error!.StatusCode, Is.EqualTo(NotFound));
    }

    [Test]
    public void RandomUsersCannotUpdateContest()
    {
        using TestContext context = this.GetServer();
        GameContest contest = this.CreateContest(context);
        GameUser randomUser = context.CreateUser();
        
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, randomUser);
        ApiResponse<ApiContestResponse>? response = client.PatchData<ApiContestResponse>("/api/v3/contests/ut", new ApiContestRequest
        {
            OrganizerId = contest.Organizer.UserId.ToString(),
            ContestTag = "#ut2",
        }, false, true);
        
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Success, Is.False);
        Assert.That(response.Error, Is.Not.Null);
        Assert.That(response.Error!.StatusCode, Is.EqualTo(Forbidden));
    }

    [Test]
    public void LevelsShowUpInCategory()
    {
        using TestContext context = this.GetServer(false);
        GameUser user = context.CreateUser();
        
        context.Time.TimestampMilliseconds = 500;
        TokenGame[] allowedGames = [TokenGame.LittleBigPlanet2];
        GameContest contest = this.CreateContest(context, allowedGames:allowedGames);

        context.CreateLevel(user, "#ut level 1", TokenGame.LittleBigPlanet2);
        context.CreateLevel(user, "level #ut 2", TokenGame.LittleBigPlanet2);
        context.CreateLevel(user, "#ut level 3", TokenGame.LittleBigPlanet2);
        
        // test level without tag
        context.CreateLevel(user, "level 4", TokenGame.LittleBigPlanet2);
        
        // test levels before start date
        context.Time.TimestampMilliseconds = 0;
        context.CreateLevel(user, "#ut before start");

        // test levels after end date
        context.Time.TimestampMilliseconds = 1000;
        context.CreateLevel(user, "#ut missed deadline");
        
        // test level published in excluded game
        context.CreateLevel(user, "#ut disallowed game", TokenGame.LittleBigPlanet1);

        DatabaseList<GameLevel> levels = context.Database.GetLevelsFromContest(contest, 4, 0, user, new(TokenGame.LittleBigPlanet2));
        Assert.That(levels.TotalItems, Is.EqualTo(3));
        Assert.That(levels.Items.All(l => l.Title.Contains("#ut ")));
    }

    [Test]
    public void LevelsShowUpInGamelessCategory()
    {
        using TestContext context = this.GetServer(false);
        GameUser user = context.CreateUser();
        
        context.Time.TimestampMilliseconds = 500;
        GameContest contest = this.CreateContest(context, null);

        // test levels from various games
        context.CreateLevel(user, "#ut level 1", TokenGame.LittleBigPlanet2);
        context.CreateLevel(user, "level #ut 2", TokenGame.LittleBigPlanet3);
        context.CreateLevel(user, "#ut best level 3", TokenGame.LittleBigPlanetPSP);
        context.CreateLevel(user, "#ut this is the best one!", TokenGame.BetaBuild);
        
        // test level without tag
        context.CreateLevel(user, "level 6", TokenGame.LittleBigPlanet2);

        DatabaseList<GameLevel> levels = context.Database.GetLevelsFromContest(contest, 10, 0, user, new(TokenGame.Website));
        Assert.That(levels.TotalItems, Is.EqualTo(4));
        Assert.That(levels.Items.All(l => l.Title.Contains("#ut ")));
    }

    [Test]
    public void NewestContestIsCorrectOne()
    {
        using TestContext context = this.GetServer(false);
        // contests end after 10ms
        context.Time.TimestampMilliseconds = 0; // start at 0ms
        this.CreateContest(context, id: "0");
        
        // one contest, should definitely be 0.
        GameContest? oldestContest = context.Database.GetNewestActiveContest();
        Assert.That(oldestContest, Is.Not.Null);
        Assert.That(oldestContest!.ContestId, Is.EqualTo("0"));
        
        context.Time.TimestampMilliseconds = 5; // advance 5ms, create a new contest.
        this.CreateContest(context, id: "5");

        // at this point, 0 and 5 are both active. the newer one is 5 so it should be 5
        oldestContest = context.Database.GetNewestActiveContest();
        Assert.That(oldestContest, Is.Not.Null);
        Assert.That(oldestContest!.ContestId, Is.EqualTo("5"));
        
        // jump to 14ms, after 0 has ended
        context.Time.TimestampMilliseconds = 14;
        
        // 5 should still be the active contest
        oldestContest = context.Database.GetNewestActiveContest();
        Assert.That(oldestContest, Is.Not.Null);
        Assert.That(oldestContest!.ContestId, Is.EqualTo("5"));
        
        // jump to 15ms, 5 should be dead
        context.Time.TimestampMilliseconds = 15;
        
        // no contests should be active
        oldestContest = context.Database.GetNewestActiveContest();
        Assert.That(oldestContest, Is.Null);
    }
    
    [Test]
    public void LastActiveContestIsCorrectOne()
    {
        using TestContext context = this.GetServer(false);
        // contests end after 10ms
        context.Time.TimestampMilliseconds = 0; // start at 0ms
        this.CreateContest(context, id: "0");
        
        // should return null during an active contest
        GameContest? oldestContest = context.Database.GetLatestCompletedContest();
        Assert.That(oldestContest, Is.Null);
        
        context.Time.TimestampMilliseconds = 5; // advance 5ms, create a new contest.
        this.CreateContest(context, id: "5");
        
        // jump to 14ms, after 0 has ended
        context.Time.TimestampMilliseconds = 14;
        
        // 0 should be the last active contest
        oldestContest = context.Database.GetLatestCompletedContest();
        Assert.That(oldestContest, Is.Not.Null);
        Assert.That(oldestContest!.ContestId, Is.EqualTo("0"));
        
        // jump to 15ms, 5 should be dead
        context.Time.TimestampMilliseconds = 15;
        
        // the last active contest should be 5
        oldestContest = context.Database.GetLatestCompletedContest();
        Assert.That(oldestContest, Is.Not.Null);
        Assert.That(oldestContest!.ContestId, Is.EqualTo("5"));
    }
}