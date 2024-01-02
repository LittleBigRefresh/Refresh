using MongoDB.Bson;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Levels;

public class ScoreLeaderboardTests : GameServerTest
{
    [Test]
    public void SubmitsScore()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        SerializedScore score = new()
        {
            Host = true,
            ScoreType = 1,
            Score = 5,
        }; 
        
        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        message = client.GetAsync($"/lbp/topscores/user/{level.LevelId}/1").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        SerializedScoreList scores = message.Content.ReadAsXML<SerializedScoreList>();
        Assert.That(scores.Scores, Has.Count.EqualTo(1));
        Assert.That(scores.Scores[0].Player, Is.EqualTo(user.Username));
        Assert.That(scores.Scores[0].Score, Is.EqualTo(5));
        
        message = client.GetAsync($"/lbp/scoreboard/user/{level.LevelId}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        SerializedMultiLeaderboardResponse scoresMulti = message.Content.ReadAsXML<SerializedMultiLeaderboardResponse>();
        SerializedPlayerLeaderboardResponse singleplayerScores = scoresMulti.Scoreboards.First(s => s.PlayerCount == 1);
        Assert.That(singleplayerScores.Scores, Has.Count.EqualTo(1));
        Assert.That(singleplayerScores.Scores[0].Player, Is.EqualTo(user.Username));
        Assert.That(singleplayerScores.Scores[0].Score, Is.EqualTo(5));
    }
    
    [Test]
    public void SubmitsDeveloperScore()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        SerializedScore score = new()
        {
            Host = true,
            ScoreType = 1,
            Score = 5,
        }; 
        
        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/developer/1", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        context.Database.Refresh();
        
        message = client.GetAsync($"/lbp/topscores/developer/1/1").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        SerializedScoreList scores = message.Content.ReadAsXML<SerializedScoreList>();
        Assert.That(scores.Scores, Has.Count.EqualTo(1));
        Assert.That(scores.Scores[0].Player, Is.EqualTo(user.Username));
        Assert.That(scores.Scores[0].Score, Is.EqualTo(5));
        
        message = client.GetAsync($"/lbp/scoreboard/developer/1").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        SerializedMultiLeaderboardResponse scoresMulti = message.Content.ReadAsXML<SerializedMultiLeaderboardResponse>();
        SerializedPlayerLeaderboardResponse singleplayerScores = scoresMulti.Scoreboards.First(s => s.PlayerCount == 1);
        Assert.That(singleplayerScores.Scores, Has.Count.EqualTo(1));
        Assert.That(singleplayerScores.Scores[0].Player, Is.EqualTo(user.Username));
        Assert.That(singleplayerScores.Scores[0].Score, Is.EqualTo(5));
    }
    
    [Test]
    public void DosentGetLeaderboardForInvalidLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message2 = client.GetAsync($"/lbp/topscores/user/{int.MaxValue}/1").Result;
        Assert.That(message2.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void DosentGetMultiLeaderboardForInvalidLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message = client.GetAsync($"/lbp/scoreboard/user/{int.MaxValue}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void DoesntSubmitInvalidScore()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        SerializedScore score = new()
        {
            Host = true,
            ScoreType = 1,
            Score = -1,
        }; 
        
        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));

        context.Database.Refresh();

        List<GameSubmittedScore> scores = context.Database.GetTopScoresForLevel(level, 1, 0, 1).Items.ToList();
        Assert.That(scores, Has.Count.EqualTo(0));
    }

    [Test]
    [TestCase(1, true)]
    [TestCase(2, true)]
    [TestCase(3, true)]
    [TestCase(4, true)]
    [TestCase(7, true)]
    [TestCase(0, false)]
    [TestCase(255, false)]
    [TestCase(8, false)]
    public void DoesntSubmitInvalidScoreType(byte type, bool shouldPass)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);
        
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        SerializedScore score = new()
        {
            Host = true,
            ScoreType = type,
            Score = 69,
        };
        
        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(shouldPass ? OK : BadRequest));
    }
    
    [Test]
    public void DoesntSubmitDeveloperInvalidScore()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        SerializedScore score = new()
        {
            Host = true,
            ScoreType = 1,
            Score = -1,
        }; 
        
        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/developer/1", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));

        context.Database.Refresh();

        List<GameSubmittedScore> scores = context.Database.GetTopScoresForLevel(context.Database.GetStoryLevelById(1), 1, 0, 1).Items.ToList();
        Assert.That(scores, Has.Count.EqualTo(0));
    }
    
    [Test]
    public void DoesntSubmitsScoreToInvalidLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        SerializedScore score = new()
        {
            Host = true,
            ScoreType = 1,
            Score = 0,
        }; 
        
        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/user/{int.MaxValue}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void DoesntSubmitDeveloperScoreToInvalidLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        SerializedScore score = new()
        {
            Host = true,
            ScoreType = 1,
            Score = 0,
        }; 
        
        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/developer/-1", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }
    
    [Test]
    public void DoesntGetDeveloperMultiLeaderboardForInvalidLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message = client.GetAsync($"/lbp/scoreboard/developer/-1").Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }
    
    [Test]
    public void DoesntGetDeveloperScoresForInvalidLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message = client.GetAsync($"/lbp/topscores/developer/-1/1").Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }
    
    /// <param name="count">The number of scores to try to fetch from the database</param>
    /// <param name="expectedIndex">The expected index of the submitted score</param>
    /// <param name="leaderboardCount">How many scores should be filled into the database</param>
    /// <param name="submittedScore">The number of points the score has</param>
    private void ScoreSegmentTest(int count, int expectedIndex, int leaderboardCount, int submittedScore, int expectedCount)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        context.FillLeaderboard(level, leaderboardCount, 1);
        GameSubmittedScore score = context.SubmitScore(submittedScore, 1, level, user, TokenGame.LittleBigPlanet2);

        List<ObjectId> scores = context.Database.GetRankedScoresAroundScore(score, count)!
            .Select(s => s.score.ScoreId)
            .ToList();
        
        Assert.Multiple(() =>
        {
            Assert.That(scores, Has.Count.EqualTo(expectedCount));
            Assert.That(scores.IndexOf(score.ScoreId), Is.EqualTo(expectedIndex));
            Assert.That(scores[expectedIndex], Is.EqualTo(score.ScoreId));
        });
    }

    [Test]
    public void PlacesScoreInSegmentCorrectly()
    {
        const int count = 5;
        const int expectedIndex = count / 2;

        this.ScoreSegmentTest(count, expectedIndex, 25, 8, 5);
    }
    
    [Test]
    public void SegmentsIfLessThanCountScore()
    {
        this.ScoreSegmentTest(5, 0, 3, 6, 4);
        this.ScoreSegmentTest(5, 2, 3, 1, 4);
    }

    [Test]
    public void SeparatesLeaderboardsByType()
    {
        using TestContext context = this.GetServer();

        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        
        GameLevel level = context.CreateLevel(user1);
        
        GameSubmittedScore score1 = context.SubmitScore(0, 1, level, user1, TokenGame.LittleBigPlanet2);
        GameSubmittedScore score2 = context.SubmitScore(0, 2, level, user2, TokenGame.LittleBigPlanet2);
        
        Assert.Multiple(() =>
        {
            Assert.That(context.Database.GetTopScoresForLevel(level, 1, 0, 1).Items, Does.Not.Contain(score2));
            Assert.That(context.Database.GetTopScoresForLevel(level, 1, 0, 2).Items, Does.Not.Contain(score1));
        });
    }

    [Test]
    public void FailsWithInvalidNumber()
    {
        using TestContext context = this.GetServer(false);
        GameUser user = context.CreateUser();
        GameSubmittedScore score = context.SubmitScore(0, 1, context.CreateLevel(user), user, TokenGame.LittleBigPlanet2);
        
        Assert.That(() => context.Database.GetRankedScoresAroundScore(score, 2), Throws.ArgumentException);
    }
    
    [Test]
    public void PlayLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message = client.PostAsync($"/lbp/play/user/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        context.Database.Refresh();

        Assert.That(level.AllPlays.Count(), Is.EqualTo(1));
    }
    
    [Test]
    public void DoesntPlayInvalidLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message = client.PostAsync($"/lbp/play/user/{int.MaxValue}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void PlayLevelWithCount()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        client.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");

        HttpResponseMessage message = client.PostAsync($"/lbp/play/user/{level.LevelId}?count=2", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        context.Database.Refresh();

        Assert.That(level.AllPlays.AsEnumerable().Sum(p => p.Count), Is.EqualTo(2));
    }
    
    [Test]
    public void DoesntPlayLevelWithInvalidCount()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        client.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");

        HttpResponseMessage message = client.PostAsync($"/lbp/play/user/{level.LevelId}?count=gtgnyegth", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
        
        HttpResponseMessage message2 = client.PostAsync($"/lbp/play/user/{level.LevelId}?count=-5", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message2.StatusCode, Is.EqualTo(BadRequest));
    }
    
    [Test]
    public void DoesntPlayLevelWithCountOnMainline()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message = client.PostAsync($"/lbp/play/user/{level.LevelId}?count=3", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }
}