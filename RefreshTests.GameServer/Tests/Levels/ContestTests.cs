using Refresh.GameServer.Types.Contests;
using Refresh.GameServer.Types.UserData;

namespace RefreshTests.GameServer.Tests.Levels;

public class ContestTests : GameServerTest
{
    private const string Banner = "https://i.imgur.com/n80NswV.png";
    
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
                ContestRulesMarkdown = "# test",
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
}