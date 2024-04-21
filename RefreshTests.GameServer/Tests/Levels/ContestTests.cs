using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;
using Refresh.GameServer.Types.Contests;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Levels;

public class ContestTests : GameServerTest
{
    private const string Banner = "https://i.imgur.com/n80NswV.png";
    
    private GameContest CreateContest(TestContext context, GameUser? organizer = null)
    {
        organizer ??= context.CreateUser();
        GameContest contest = new()
        {
            ContestId = "ut",
            Organizer = organizer,
            BannerUrl = Banner,
            ContestTitle = "unit test",
            ContestSummary = "summary",
            ContestTag = "#ut",
            ContestDetails = "# unit test",
            CreationDate = context.Time.Now,
            StartDate = context.Time.Now,
            EndDate = context.Time.Now + TimeSpan.FromMilliseconds(10),
        };
        
        context.Database.CreateContest(contest);

        return contest;
    }
    
    [Test]
    public void CanCreateContest()
    {
        using TestContext context = this.GetServer(false);
        GameUser organizer = context.CreateUser();
        Assert.That(() =>
        {
            // ReSharper disable once AccessToDisposedClosure
            context.Database.CreateContest(new GameContest
            {
                ContestId = "contest",
                Organizer = organizer,
                BannerUrl = Banner,
                ContestTitle = "The Contest Contest",
                ContestSummary = "a contest about contests",
                ContestTag = "#cc1",
                ContestDetails = "# test",
                CreationDate = DateTimeOffset.FromUnixTimeMilliseconds(0),
                StartDate = DateTimeOffset.FromUnixTimeMilliseconds(1),
                EndDate = DateTimeOffset.FromUnixTimeMilliseconds(2),
            });
        }, Throws.Nothing);

        GameContest? contest = context.Database.GetContestById("contest");
        Assert.That(contest, Is.Not.Null);
        Assert.That(contest.ContestTitle, Is.EqualTo("The Contest Contest"));
        Assert.That(contest.Organizer, Is.EqualTo(organizer));
    }

    [Test]
    public void CanCreateContestFromApi()
    {
        using TestContext context = this.GetServer();
        GameUser organizer = context.CreateUser();
        GameUser admin = context.CreateAdmin();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, admin);
        ApiResponse<ApiContestResponse>? response = client.PostData<ApiContestResponse>("/api/v3/admin/contests/cbt1", new ApiContestRequest
        {
            OrganizerId = organizer.UserId.ToString(),
            CreationDate = context.Time.Now,
            StartDate = context.Time.Now + TimeSpan.FromHours(1),
            EndDate = context.Time.Now + TimeSpan.FromHours(2),
            ContestTag = "#cbt1",
            BannerUrl = Banner,
            ContestTitle = "The You-Know-What Contest #1",
            ContestSummary = "The contest all about *that*",
            ContestDetails = "Yep Yep Yep Yep Yep",
        });
        
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Success, Is.True);

        GameContest? createdContest = context.Database.GetContestById("cbt1");
        Assert.That(createdContest, Is.Not.Null);
        Assert.That(createdContest.Organizer, Is.EqualTo(organizer));
    }
    
    [Test]
    public void CanUpdateContestFromApi()
    {
        using TestContext context = this.GetServer();
        GameUser admin = context.CreateAdmin();
        GameContest contest = this.CreateContest(context);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, admin);
        ApiResponse<ApiContestResponse>? response = client.PatchData<ApiContestResponse>("/api/v3/contests/ut", new ApiContestRequest
        {
            OrganizerId = contest.Organizer.UserId.ToString(),
            ContestTag = "#ut2",
        });
        
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Success, Is.True);

        GameContest? createdContest = context.Database.GetContestById("ut");
        Assert.That(createdContest, Is.Not.Null);
        Assert.That(createdContest.ContestTag, Is.EqualTo("#ut2"));
    }

    [Test]
    public void UpdateApiIsSecure()
    {
        using TestContext context = this.GetServer();
        GameContest contest = this.CreateContest(context);
        GameUser randomUser = context.CreateUser();
        
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, randomUser);
        ApiResponse<ApiContestResponse>? response = client.PatchData<ApiContestResponse>("/api/v3/contests/ut", new ApiContestRequest
        {
            OrganizerId = contest.Organizer.UserId.ToString(),
            ContestTag = "#ut2",
        });
        
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Success, Is.False);
        Assert.That(response.Error, Is.Not.Null);
        Assert.That(response.Error.StatusCode, Is.EqualTo(Forbidden));
    }

    [Test]
    public void LevelsShowUpInCategory()
    {
        using TestContext context = this.GetServer(false);
        GameUser user = context.CreateUser();
        
        context.Time.TimestampMilliseconds = 500;
        GameContest contest = this.CreateContest(context);

        context.CreateLevel(user, "#ut level 1");
        context.CreateLevel(user, "level #ut 2");
        context.CreateLevel(user, "#ut level 3");
        context.CreateLevel(user, "level 4");
        
        // test levels before start date
        context.Time.TimestampMilliseconds = 0;
        context.CreateLevel(user, "#ut before start");

        // test levels after end date
        context.Time.TimestampMilliseconds = 1000;
        context.CreateLevel(user, "#ut missed deadline");

        DatabaseList<GameLevel> levels = context.Database.GetLevelsFromContest(contest, 4, 0, user, new LevelFilterSettings(TokenGame.LittleBigPlanet2));
        Assert.That(levels.TotalItems, Is.EqualTo(3));
        Assert.That(levels.Items.All(l => l.Title.Contains("#ut ")));
    }
}