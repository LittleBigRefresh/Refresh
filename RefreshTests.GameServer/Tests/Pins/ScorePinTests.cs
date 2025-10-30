using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Pins;
using Refresh.Database.Models.Relations;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Types.UserData.Leaderboard;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Pins;

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
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        long pinIdToCheck = isStoryLevel ? (long)ServerPins.TopXOfAnyStoryLevelWithOver50Scores
                                         : (long)ServerPins.TopXOfAnyCommunityLevelWithOver50Scores;
        
        GameLevel level = isStoryLevel ? context.Database.GetStoryLevelById(1) : context.CreateLevel(user);
        int levelId = isStoryLevel ? level.StoryId : level.LevelId;
        string slotType = level.SlotType.ToGameType();

        // Prepare by posting 100 scores by other people
        context.FillLeaderboard(level, 100, scoreType);

        // ROUND 1: Post our initial score to create a pin relation
        SerializedScore score1 = new()
        {
            Host = true,
            ScoreType = scoreType,
            Score = 10,
            PlayerUsernames = [user.Username],
        };

        context.Database.PlayLevel(level, user, 1);
        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/{slotType}/{levelId}", new StringContent(score1.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Ensure we now have the pin
        PinProgressRelation? relation = context.Database.GetUserPinProgress(pinIdToCheck, user, false, TokenPlatform.PS3);
        Assert.That(relation, Is.Not.Null);
        int progress = relation!.Progress;

        context.Database.Refresh();

        // ROUND 2: Now post a better score to update our pin relation
        SerializedScore score2 = new()
        {
            Host = true,
            ScoreType = scoreType,
            Score = 50,
            PlayerUsernames = [user.Username],
        };

        message = client.PostAsync($"/lbp/scoreboard/{slotType}/{levelId}", new StringContent(score2.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Ensure the pin now has a better progress value (is smaller)
        relation = context.Database.GetUserPinProgress(pinIdToCheck, user, false, TokenPlatform.PS3);
        Assert.That(relation, Is.Not.Null);
        Assert.That(relation!.Progress, Is.LessThan(progress));

        // Ensure the pin's progress is above 0
        Assert.That(relation!.Progress, Is.GreaterThan(0));
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
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        long pinIdToCheck = isStoryLevel ? (long)ServerPins.TopFourthOfXStoryLevelsWithOver50Scores
                                         : (long)ServerPins.TopFourthOfXCommunityLevelsWithOver50Scores;

        // ROUND 1: Adding the pin
        // Create a level and spam it with scores by others
        GameLevel level = isStoryLevel ? context.Database.GetStoryLevelById(1) : context.CreateLevel(user);
        int levelId = isStoryLevel ? level.StoryId : level.LevelId;
        string slotType = level.SlotType.ToGameType();

        context.FillLeaderboard(level, 100, scoreType);

        // Now post our score which will definitely make it to the top 25%
        SerializedScore score = new()
        {
            Host = true,
            ScoreType = scoreType,
            Score = 80,
            PlayerUsernames = [user.Username],
        };

        context.Database.PlayLevel(level, user, 1);
        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/{slotType}/{levelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Ensure we now have the pin
        PinProgressRelation? relation = context.Database.GetUserPinProgress(pinIdToCheck, user, false, TokenPlatform.PS3);
        Assert.That(relation, Is.Not.Null);
        Assert.That(relation!.Progress, Is.EqualTo(1));

        // ROUND 2: Updating the pin
        // Create another level and spam it with scores by others aswell
        GameLevel level2 = isStoryLevel ? context.Database.GetStoryLevelById(2) : context.CreateLevel(user);
        int levelId2 = isStoryLevel ? level2.StoryId : level2.LevelId;

        context.FillLeaderboard(level2, 100, scoreType);

        // Now post our score which will definitely make it to the top 25% here aswell
        SerializedScore score2 = new()
        {
            Host = true,
            ScoreType = scoreType,
            Score = 80,
            PlayerUsernames = [user.Username],
        };

        context.Database.PlayLevel(level2, user, 1);
        message = client.PostAsync($"/lbp/scoreboard/{slotType}/{levelId2}", new StringContent(score2.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        context.Database.Refresh();

        // Ensure the pin progress has been incremented
        PinProgressRelation? relation2 = context.Database.GetUserPinProgress(pinIdToCheck, user, false, TokenPlatform.PS3);
        Assert.That(relation2, Is.Not.Null);
        Assert.That(relation2!.Progress, Is.EqualTo(2));
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
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        long pinIdToCheck = isStoryLevel ? (long)ServerPins.TopFourthOfXStoryLevelsWithOver50Scores
                                         : (long)ServerPins.TopFourthOfXCommunityLevelsWithOver50Scores;
        
        GameLevel level = isStoryLevel ? context.Database.GetStoryLevelById(1) : context.CreateLevel(user);
        int levelId = isStoryLevel ? level.StoryId : level.LevelId;
        string slotType = level.SlotType.ToGameType();

        // Prepare by posting only 10 scores by other people (less than 50)
        context.FillLeaderboard(level, 10, scoreType);

        // Post our own score
        SerializedScore score = new()
        {
            Host = true,
            ScoreType = scoreType,
            Score = 10,
            PlayerUsernames = [user.Username],
        };

        context.Database.PlayLevel(level, user, 1);
        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/{slotType}/{levelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Ensure we don't have the pin
        PinProgressRelation? relation = context.Database.GetUserPinProgress(pinIdToCheck, user, false, TokenPlatform.PS3);
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
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        long pinIdToCheck = isStoryLevel ? (long)ServerPins.TopFourthOfXStoryLevelsWithOver50Scores
                                         : (long)ServerPins.TopFourthOfXCommunityLevelsWithOver50Scores;
        
        GameLevel level = isStoryLevel ? context.Database.GetStoryLevelById(1) : context.CreateLevel(user);
        int levelId = isStoryLevel ? level.StoryId : level.LevelId;
        string slotType = level.SlotType.ToGameType();

        // Prepare by posting 100 scores by other people
        context.FillLeaderboard(level, 100, scoreType);

        // Post a score which will definitely not make it to the top 25%
        SerializedScore score = new()
        {
            Host = true,
            ScoreType = scoreType,
            Score = 10,
            PlayerUsernames = [user.Username],
        };

        context.Database.PlayLevel(level, user, 1);
        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/{slotType}/{levelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Ensure we don't have the pin
        PinProgressRelation? relation = context.Database.GetUserPinProgress(pinIdToCheck, user, false, TokenPlatform.PS3);
        Assert.That(relation, Is.Null);
    }
}