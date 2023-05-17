using MongoDB.Bson;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace RefreshTests.GameServer.Tests;

public class ScoreLeaderboardTests : GameServerTest
{
    [Test]
    public async Task SubmitsScore()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        string scorePayload = $@"<playRecord>
<host>true</host>
<type>1</type>
<playerIds>{user.Username}</playerIds>
<score>0</score>
</playRecord>";
        
        HttpResponseMessage message = await client.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(scorePayload));
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        context.Database.Refresh();

        List<GameSubmittedScore> scores = context.Database.GetTopScoresForLevel(level, 1, 0, 1).ToList();
        Assert.That(scores, Has.Count.EqualTo(1));
    }
    
    /// <param name="count">The number of scores to try to fetch from the database</param>
    /// <param name="expectedIndex">The expected index of the submitted score</param>
    /// <param name="leaderboardCount">How many scores should be filled into the database</param>
    /// <param name="submittedScore">The number of points the score has</param>
    private void ScoreSegmentTest(int count, int expectedIndex, int leaderboardCount, int submittedScore)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        context.FillLeaderboard(level, leaderboardCount, 1);
        GameSubmittedScore score = context.SubmitScore(submittedScore, 1, level, user);

        List<ObjectId> scores = context.Database.GetRankedScoresAroundScore(score, count)!
            .Select(s => s.score.ScoreId)
            .ToList();
        
        Assert.Multiple(() =>
        {
            Assert.That(scores.IndexOf(score.ScoreId), Is.EqualTo(expectedIndex));
            Assert.That(scores[expectedIndex], Is.EqualTo(score.ScoreId));
        });
    }

    [Test]
    public void PlacesScoreInSegmentCorrectly()
    {
        const int count = 5;
        const int expectedIndex = count / 2;

        this.ScoreSegmentTest(count, expectedIndex, 25, 8);
    }
    
    [Test]
    public void SegmentsIfLessThanCountScore()
    {
        this.ScoreSegmentTest(5, 0, 4, 6);
        this.ScoreSegmentTest(5, 2, 4, 1);
    }

    [Test]
    public void SeparatesLeaderboardsByType()
    {
        using TestContext context = this.GetServer();

        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        
        GameLevel level = context.CreateLevel(user1);
        
        GameSubmittedScore score1 = context.SubmitScore(0, 1, level, user1);
        GameSubmittedScore score2 = context.SubmitScore(0, 2, level, user2);
        Assert.Multiple(() =>
        {
            Assert.That(context.Database.GetTopScoresForLevel(level, 1, 0, 1), Does.Not.Contain(score2));
            Assert.That(context.Database.GetTopScoresForLevel(level, 1, 0, 2), Does.Not.Contain(score1));
        });
    }
}