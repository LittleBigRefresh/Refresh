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
    [TestCase(1)]
    [TestCase(2)]
    public void AchieveTopXOfCommunityLeaderboardsPin(byte scoreType)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        // Prepare by posting 100 scores by other people
        context.FillLeaderboard(level, 100, scoreType);

        // Ensure the level now has 100 unique scores
        DatabaseList<ScoreWithRank> scores = context.Database.GetTopScoresForLevel(level, 100, 0, scoreType);
        Assert.That(scores.TotalItems, Is.EqualTo(100));

        // Now post our score
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        SerializedScore score = new()
        {
            Host = true,
            ScoreType = scoreType,
            Score = 10,
        };

        context.Database.PlayLevel(level, user, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Ensure we now have the pin
        PinProgressRelation? relation = context.Database.GetUserPinProgress((long)ManuallyAwardedPins.TopXOfAnyCommunityLevelWithOver50Scores, user, false);
        Assert.That(relation, Is.Not.Null);
        int progress = relation!.Progress;

        // Now post a better score to try and update our pin progress
        score = new()
        {
            Host = true,
            ScoreType = scoreType,
            Score = 50,
        };

        message = client.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Ensure the pin has a better progress value
        relation = context.Database.GetUserPinProgress((long)ManuallyAwardedPins.TopXOfAnyCommunityLevelWithOver50Scores, user, false);
        Assert.That(relation, Is.Not.Null);
        Assert.That(relation!.Progress, Is.LessThan(progress));
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public void RejectTopXOfCommunityLeaderboardsPinIfTooFewScores(byte scoreType)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        // Prepare by posting only 10 scores by other people
        context.FillLeaderboard(level, 10, scoreType);

        // Ensure the level now has 10 unique scores
        DatabaseList<ScoreWithRank> scores = context.Database.GetTopScoresForLevel(level, 100, 0, scoreType);
        Assert.That(scores.TotalItems, Is.EqualTo(10));

        // Now post our score
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        SerializedScore score = new()
        {
            Host = true,
            ScoreType = scoreType,
            Score = 10,
        };

        context.Database.PlayLevel(level, user, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Ensure we don't have the pin
        PinProgressRelation? relation = context.Database.GetUserPinProgress((long)ManuallyAwardedPins.TopXOfAnyCommunityLevelWithOver50Scores, user, false);
        Assert.That(relation, Is.Null);
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public void AchieveTopXOfStoryLeaderboardsPin(byte scoreType)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.Database.GetStoryLevelById(1);

        // Prepare by posting 100 scores by other people
        context.FillLeaderboard(level, 100, scoreType);

        // Ensure the level now has 100 unique scores
        DatabaseList<ScoreWithRank> scores = context.Database.GetTopScoresForLevel(level, 100, 0, scoreType);
        Assert.That(scores.TotalItems, Is.EqualTo(100));

        // Now post our score
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        SerializedScore score = new()
        {
            Host = true,
            ScoreType = scoreType,
            Score = 10,
        };

        context.Database.PlayLevel(level, user, 1);

        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/developer/{level.StoryId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Ensure we now have the pin
        PinProgressRelation? relation = context.Database.GetUserPinProgress((long)ManuallyAwardedPins.TopXOfAnyStoryLevelWithOver50Scores, user, false);
        Assert.That(relation, Is.Not.Null);
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public void RejectTopXOfStoryLeaderboardsPinIfTooFewScores(byte scoreType)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.Database.GetStoryLevelById(1);

        // Prepare by posting only 10 scores by other people
        context.FillLeaderboard(level, 10, scoreType);

        // Ensure the level now has 10 unique scores
        DatabaseList<ScoreWithRank> scores = context.Database.GetTopScoresForLevel(level, 100, 0, scoreType);
        Assert.That(scores.TotalItems, Is.EqualTo(10));

        // Now post our score
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        SerializedScore score = new()
        {
            Host = true,
            ScoreType = scoreType,
            Score = 10,
        };

        context.Database.PlayLevel(level, user, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/developer/{level.StoryId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Ensure we don't have the pin
        PinProgressRelation? relation = context.Database.GetUserPinProgress((long)ManuallyAwardedPins.TopXOfAnyStoryLevelWithOver50Scores, user, false);
        Assert.That(relation, Is.Null);
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public void AchieveTopFourthOfXCommunityLeaderboardsPin(byte scoreType)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        // Prepare by posting 100 scores by other people
        context.FillLeaderboard(level, 100, scoreType);

        // Ensure the level now has 100 unique scores
        DatabaseList<ScoreWithRank> scores = context.Database.GetTopScoresForLevel(level, 100, 0, scoreType);
        Assert.That(scores.TotalItems, Is.EqualTo(100));

        // Now post our score, which will beat most other scores
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        SerializedScore score = new()
        {
            Host = true,
            ScoreType = scoreType,
            Score = 420,
        };

        context.Database.PlayLevel(level, user, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Ensure we have the pin
        PinProgressRelation? relation = context.Database.GetUserPinProgress((long)ManuallyAwardedPins.TopFourthOfXCommunityLevelsWithOver50Scores, user, false);
        Assert.That(relation, Is.Not.Null);
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public void RejectTopFourthOfXCommunityLeaderboardsPinIfSkillIssue(byte scoreType)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        // Prepare by posting 100 scores by other people
        context.FillLeaderboard(level, 100, scoreType);

        // Ensure the level now has 100 unique scores
        DatabaseList<ScoreWithRank> scores = context.Database.GetTopScoresForLevel(level, 100, 0, scoreType);
        Assert.That(scores.TotalItems, Is.EqualTo(100));

        // Now post our score, which definitely won't make it to the top 25%
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        SerializedScore score = new()
        {
            Host = true,
            ScoreType = scoreType,
            Score = 5,
        };

        context.Database.PlayLevel(level, user, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Ensure we don't have the pin
        PinProgressRelation? relation = context.Database.GetUserPinProgress((long)ManuallyAwardedPins.TopFourthOfXCommunityLevelsWithOver50Scores, user, false);
        Assert.That(relation, Is.Null);
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public void AchieveTopFourthOfXStoryLeaderboardsPin(byte scoreType)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.Database.GetStoryLevelById(1);

        // Prepare by posting 100 scores by other people
        context.FillLeaderboard(level, 100, scoreType);

        // Ensure the level now has 100 unique scores
        DatabaseList<ScoreWithRank> scores = context.Database.GetTopScoresForLevel(level, 100, 0, scoreType);
        Assert.That(scores.TotalItems, Is.EqualTo(100));

        // Now post our score, which will beat most other scores
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        SerializedScore score = new()
        {
            Host = true,
            ScoreType = scoreType,
            Score = 420,
        };

        context.Database.PlayLevel(level, user, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/developer/{level.StoryId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Ensure we have the pin
        PinProgressRelation? relation = context.Database.GetUserPinProgress((long)ManuallyAwardedPins.TopFourthOfXStoryLevelsWithOver50Scores, user, false);
        Assert.That(relation, Is.Not.Null);
    }

    [Test]
    [TestCase(1)]
    [TestCase(2)]
    public void RejectTopFourthOfXStoryLeaderboardsPinIfSkillIssue(byte scoreType)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.Database.GetStoryLevelById(1);

        // Prepare by posting 100 scores by other people
        context.FillLeaderboard(level, 100, scoreType);

        // Ensure the level now has 100 unique scores
        DatabaseList<ScoreWithRank> scores = context.Database.GetTopScoresForLevel(level, 100, 0, scoreType);
        Assert.That(scores.TotalItems, Is.EqualTo(100));

        // Now post our score, which definitely won't make it to the top 25%
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        SerializedScore score = new()
        {
            Host = true,
            ScoreType = scoreType,
            Score = 5,
        };

        context.Database.PlayLevel(level, user, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/developer/{level.StoryId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Ensure we don't have the pin
        PinProgressRelation? relation = context.Database.GetUserPinProgress((long)ManuallyAwardedPins.TopFourthOfXStoryLevelsWithOver50Scores, user, false);
        Assert.That(relation, Is.Null);
    }
}