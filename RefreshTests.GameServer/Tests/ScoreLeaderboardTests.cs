using MongoDB.Bson;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Endpoints.Game.Levels;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace RefreshTests.GameServer.Tests;

public class ScoreLeaderboardTests : GameServerTest
{
    [Test]
    public async Task SubmitsScore()
    {
        TestContext context = this.GetServer();
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

        List<GameSubmittedScore> scores = context.Database.GetTopScoresForLevel(level, 1, 0).ToList();
        Assert.That(scores, Has.Count.EqualTo(1));
    }

    [Test]
    public void PlacesScoreInSegmentCorrectly()
    {
        TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        context.FillLeaderboard(level, 25);
        GameSubmittedScore score = context.SubmitScore(8, level, user);

        const int count = 5;
        const int expectedIndex = count / 2;

        List<ObjectId> scores = context.Database.GetRankedScoresAroundScore(score, count)!
            .Select(s => s.score.ScoreId)
            .ToList();
        
        Assert.Multiple(() =>
        {
            Assert.That(scores.IndexOf(score.ScoreId), Is.EqualTo(expectedIndex));
            Assert.That(scores[expectedIndex], Is.EqualTo(score.ScoreId));
        });
    }
}