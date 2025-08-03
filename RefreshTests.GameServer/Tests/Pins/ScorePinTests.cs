using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Database.Models.Pins;
using Refresh.Database.Models.Relations;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Types.UserData.Leaderboard;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Levels;

public class ScorePinTests : GameServerTest
{
    [Test]
    [TestCase(1, false)]
    [TestCase(2, false)]
    [TestCase(1, true)]
    [TestCase(2, true)]
    public void AchieveTopXOfLeaderboardsPin(byte scoreType, bool isStoryLevel)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = isStoryLevel ? context.Database.GetStoryLevelById(1) : context.CreateLevel(user);
        int levelId = isStoryLevel ? level.StoryId : level.LevelId;
        string slotType = level.SlotType.ToGameType();
        long pinIdToCheck = isStoryLevel ? (long)ManuallyAwardedPins.TopXOfAnyStoryLevelWithOver50Scores
                                         : (long)ManuallyAwardedPins.TopXOfAnyCommunityLevelWithOver50Scores;

        // Prepare by posting 100 scores by other people
        context.FillLeaderboard(level, 100, scoreType);

        // Ensure the level now has 100 unique scores
        DatabaseList<ScoreWithRank> scores = context.Database.GetTopScoresForLevel(level, 100, 0, scoreType);
        Assert.That(scores.TotalItems, Is.EqualTo(100));

        // ROUND 1: Post our initial score to create the pin relation
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        SerializedScore score1 = new()
        {
            Host = true,
            ScoreType = scoreType,
            Score = 10,
        };

        context.Database.PlayLevel(level, user, 1);

        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/{slotType}/{levelId}", new StringContent(score1.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Ensure we now have the pin
        PinProgressRelation? relation = context.Database.GetUserPinProgress(pinIdToCheck, user, false);
        Assert.That(relation, Is.Not.Null);
        int progress = relation!.Progress;

        // ROUND 2: Now post a better score to update our pin relation
        SerializedScore score2 = new()
        {
            Host = true,
            ScoreType = scoreType,
            Score = 50,
        };

        message = client.PostAsync($"/lbp/scoreboard/{slotType}/{levelId}", new StringContent(score2.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Ensure the pin now has a better progress value (is smaller)
        context.Database.Refresh();
        relation = context.Database.GetUserPinProgress(pinIdToCheck, user, false);
        Assert.That(relation, Is.Not.Null);
        Assert.That(relation!.Progress, Is.LessThan(progress));
    }

    [Test]
    [TestCase(1, false)]
    [TestCase(2, false)]
    [TestCase(1, true)]
    [TestCase(2, true)]
    public void AchieveTopFourthOfXLeaderboardsPin(byte scoreType, bool isStoryLevel)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = isStoryLevel ? context.Database.GetStoryLevelById(1) : context.CreateLevel(user);
        int levelId = isStoryLevel ? level.StoryId : level.LevelId;
        string slotType = level.SlotType.ToGameType();
        long pinIdToCheck = isStoryLevel ? (long)ManuallyAwardedPins.TopXOfAnyStoryLevelWithOver50Scores
                                         : (long)ManuallyAwardedPins.TopXOfAnyCommunityLevelWithOver50Scores;

        // Prepare by posting 100 scores by other people
        context.FillLeaderboard(level, 100, scoreType);

        // Ensure the level now has 100 unique scores
        DatabaseList<ScoreWithRank> scores = context.Database.GetTopScoresForLevel(level, 100, 0, scoreType);
        Assert.That(scores.TotalItems, Is.EqualTo(100));

        // ROUND 1: Post our score which will definitely make it to the top 25%
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        SerializedScore score1 = new()
        {
            Host = true,
            ScoreType = scoreType,
            Score = 80,
        };

        context.Database.PlayLevel(level, user, 1);

        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/{slotType}/{levelId}", new StringContent(score1.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Ensure we now have the pin
        PinProgressRelation? relation = context.Database.GetUserPinProgress(pinIdToCheck, user, false);
        Assert.That(relation, Is.Not.Null);
        int progress = relation!.Progress;

        // ROUND 2: Now post a better score to update our pin relation
        SerializedScore score2 = new()
        {
            Host = true,
            ScoreType = scoreType,
            Score = 1000,
        };

        message = client.PostAsync($"/lbp/scoreboard/{slotType}/{levelId}", new StringContent(score2.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Ensure the pin now has a better progress value (is smaller)
        context.Database.Refresh();
        relation = context.Database.GetUserPinProgress(pinIdToCheck, user, false);
        Assert.That(relation, Is.Not.Null);
        Assert.That(relation!.Progress, Is.LessThan(progress));

        // Ensure that progress is higher than 0
        Assert.That(relation!.Progress, Is.GreaterThan(0));
    }

    [Test]
    [TestCase(1, false)]
    [TestCase(2, false)]
    [TestCase(1, true)]
    [TestCase(2, true)]
    public void RejectTopXOfLeaderboardsPinIfTooFewScores(byte scoreType, bool isStoryLevel)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = isStoryLevel ? context.Database.GetStoryLevelById(1) : context.CreateLevel(user);
        int levelId = isStoryLevel ? level.StoryId : level.LevelId;
        string slotType = level.SlotType.ToGameType();
        long pinIdToCheck = isStoryLevel ? (long)ManuallyAwardedPins.TopFourthOfXStoryLevelsWithOver50Scores
                                         : (long)ManuallyAwardedPins.TopFourthOfXCommunityLevelsWithOver50Scores;

        // Prepare by posting 10 scores by other people
        context.FillLeaderboard(level, 10, scoreType);

        // Ensure the level now has only 10 unique scores (less than 50)
        DatabaseList<ScoreWithRank> scores = context.Database.GetTopScoresForLevel(level, 100, 0, scoreType);
        Assert.That(scores.TotalItems, Is.EqualTo(10));

        // Post our own score
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        SerializedScore score = new()
        {
            Host = true,
            ScoreType = scoreType,
            Score = 10,
        };

        context.Database.PlayLevel(level, user, 1);

        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/{slotType}/{levelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Ensure we don't have the pin
        PinProgressRelation? relation = context.Database.GetUserPinProgress(pinIdToCheck, user, false);
        Assert.That(relation, Is.Null);
    }

    [Test]
    [TestCase(1, false)]
    [TestCase(2, false)]
    [TestCase(1, true)]
    [TestCase(2, true)]
    public void RejectTopFourthOfXLeaderboardsPinIfSkillIssue(byte scoreType, bool isStoryLevel)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = isStoryLevel ? context.Database.GetStoryLevelById(1) : context.CreateLevel(user);
        int levelId = isStoryLevel ? level.StoryId : level.LevelId;
        string slotType = level.SlotType.ToGameType();
        long pinIdToCheck = isStoryLevel ? (long)ManuallyAwardedPins.TopFourthOfXStoryLevelsWithOver50Scores
                                         : (long)ManuallyAwardedPins.TopFourthOfXCommunityLevelsWithOver50Scores;

        // Prepare by posting 100 scores by other people
        context.FillLeaderboard(level, 100, scoreType);

        // Ensure the level now has 100 unique scores to try to create the pin relation
        DatabaseList<ScoreWithRank> scores = context.Database.GetTopScoresForLevel(level, 100, 0, scoreType);
        Assert.That(scores.TotalItems, Is.EqualTo(100));

        // Post a score which will definitely not make it to the top 25%
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        SerializedScore score = new()
        {
            Host = true,
            ScoreType = scoreType,
            Score = 10,
        };

        context.Database.PlayLevel(level, user, 1);

        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/{slotType}/{levelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Ensure we don't have the pin
        PinProgressRelation? relation = context.Database.GetUserPinProgress(pinIdToCheck, user, false);
        Assert.That(relation, Is.Null);
    }
}