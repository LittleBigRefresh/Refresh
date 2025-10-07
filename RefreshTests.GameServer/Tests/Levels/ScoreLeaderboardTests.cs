using MongoDB.Bson;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using RefreshTests.GameServer.Extensions;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Notifications;
using Refresh.Interfaces.Game.Types.Lists;
using Refresh.Interfaces.Game.Types.UserData.Leaderboard;

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
            PlayerUsernames = [user.Username],
        }; 

        HttpResponseMessage message = client.PostAsync($"/lbp/play/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        message = client.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
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
            PlayerUsernames = [user.Username],
        }; 

        HttpResponseMessage message = client.PostAsync($"/lbp/play/developer/1", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        message = client.PostAsync($"/lbp/scoreboard/developer/1", new StringContent(score.AsXML())).Result;
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
    public void RemoveMissingPlayersFromScoreSubmission()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        SerializedScore submission = new()
        {
            Host = true,
            ScoreType = 2,
            Score = 5,
            PlayerUsernames = [user.Username, "I'm a bacon griller"],
        }; 

        HttpResponseMessage message = client.PostAsync($"/lbp/play/user/{level.LevelId}", null).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        message = client.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(submission.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        ScoreWithRank? score = context.Database.GetTopScoresForLevel(level, 1000, 0, 2, false).Items.FirstOrDefault(s => s.score.PublisherId == user.UserId);
        Assert.That(score, Is.Not.Null);
        Assert.That(score!.score.PlayerIds.Count, Is.EqualTo(1));
        Assert.That(score!.score.PlayerIds.First(), Is.EqualTo(user.UserId));
    }

    [Test]
    public void IgnorePlayerListAndScoreTypeForPSP()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        client.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");

        HttpResponseMessage message = client.PostAsync($"/lbp/play/user/{level.LevelId}?count=1", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedScore submission = new()
        {
            Host = true,
            ScoreType = 30,
            Score = 5,
            PlayerUsernames = [],
        };
        
        message = client.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(submission.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        ScoreWithRank? score = context.Database.GetTopScoresForLevel(level, 1000, 0, 1, false).Items.FirstOrDefault(s => s.score.PublisherId == user.UserId);
        Assert.That(score, Is.Not.Null);
        Assert.That(score!.score.PlayerIds.Count, Is.EqualTo(1));
        Assert.That(score!.score.PlayerIds.First(), Is.EqualTo(user.UserId));
    }
    
    [Test]
    public void DoesntGetLeaderboardForInvalidLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message2 = client.GetAsync($"/lbp/topscores/user/{int.MaxValue}/1").Result;
        Assert.That(message2.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void DoesntGetMultiLeaderboardForInvalidLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message = client.GetAsync($"/lbp/scoreboard/user/{int.MaxValue}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    [TestCase(-1)]
    [TestCase(int.MaxValue)]
    public void DoesntSubmitInvalidScore(int scoreValue)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        SerializedScore score = new()
        {
            Host = true,
            ScoreType = 1,
            Score = scoreValue,
            PlayerUsernames = [user.Username],
        }; 
        
        HttpResponseMessage message = client.PostAsync($"/lbp/play/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        message = client.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));

        context.Database.Refresh();

        List<ScoreWithRank> scores = context.Database.GetTopScoresForLevel(level, 1, 0, 1).Items.ToList();
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
            PlayerUsernames = [user.Username],
        };
        
        HttpResponseMessage message = client.PostAsync($"/lbp/play/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        message = client.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(shouldPass ? OK : BadRequest));
    }
    
    [Test]
    [TestCase(-1)]
    [TestCase(int.MaxValue)]
    public void DoesntSubmitDeveloperInvalidScore(int scoreValue)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        SerializedScore score = new()
        {
            Host = true,
            ScoreType = 1,
            Score = scoreValue,
            PlayerUsernames = [user.Username],
        }; 
        
        HttpResponseMessage message = client.PostAsync($"/lbp/play/developer/1", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        message = client.PostAsync($"/lbp/scoreboard/developer/1", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));

        context.Database.Refresh();

        List<ScoreWithRank> scores = context.Database.GetTopScoresForLevel(context.Database.GetStoryLevelById(1), 1, 0, 1).Items.ToList();
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
            PlayerUsernames = [user.Username],
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
            PlayerUsernames = [user.Username],
        }; 
        
        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/developer/-1", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }

    [Test]
    public void DoesntSubmitScoreWithoutPublisherAsPlayer()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        SerializedScore score = new()
        {
            Host = true,
            ScoreType = 1,
            Score = 0,
            PlayerUsernames = [],
        }; 
        
        HttpResponseMessage message = client.PostAsync($"/lbp/play/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        message = client.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }

    [Test]
    public void DoesntSubmitScoreWithTooManyPlayers()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        SerializedScore score = new()
        {
            Host = true,
            ScoreType = 1,
            Score = 0,
            PlayerUsernames = [user.Username, "Hi", "Hi", "Hi", "Hi", "Hi", "Hi", "Hi", "Hi", "Hi I'm also here"],
        }; 
        
        HttpResponseMessage message = client.PostAsync($"/lbp/play/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        message = client.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }

    [Test]
    public void DoesntSubmitScoreWithInvalidPlayerCountToScoreTypeRatio()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        SerializedScore score = new()
        {
            Host = true,
            ScoreType = 1, // Only one player in total but 4 online players
            Score = 0,
            PlayerUsernames = [user.Username, "Hi", "Hi", "Hi"],
        }; 
        
        HttpResponseMessage message = client.PostAsync($"/lbp/play/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        message = client.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }

    [Test]
    public void DoesntSubmitScoreToUnplayedLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        SerializedScore score = new()
        {
            Host = true,
            ScoreType = 1,
            Score = 0,
            PlayerUsernames = [user.Username],
        }; 
        
        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(Unauthorized));
    }
    
    [Test]
    public void DoesntSubmitScoreToUnplayedDeveloperLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        SerializedScore score = new()
        {
            Host = true,
            ScoreType = 1,
            Score = 0,
            PlayerUsernames = [user.Username],
        }; 
        
        HttpResponseMessage message = client.PostAsync($"/lbp/scoreboard/developer/1", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(Unauthorized));
    }
    
    [Test]
    public void DoesntGetDeveloperMultiLeaderboardForInvalidLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message = client.GetAsync($"/lbp/scoreboard/developer/-1").Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void DoesntGetDeveloperScoresForInvalidLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message = client.GetAsync($"/lbp/topscores/developer/-1/1").Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }

    [Test]
    public void ShowsAllScoresForVersusIngame()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        // Upload some scores for various types
        for (byte i = 1; i <= 4; i++)
        {
            GameUser uploader = context.CreateUser();
            context.Database.SubmitScore(new SerializedScore()
            {
                Score = 180,
                ScoreType = i,
                PlayerUsernames = [uploader.Username],
            }, uploader, level, TokenGame.LittleBigPlanet2, TokenPlatform.PS3, [uploader]);
        }

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        // Check for all normal score types
        for (byte i = 1; i <= 4; i++)
        {
            HttpResponseMessage message = client.GetAsync($"/lbp/topscores/user/{level.LevelId}/{i}").Result;
            Assert.That(message.StatusCode, Is.EqualTo(OK));
            
            SerializedScoreList scores = message.Content.ReadAsXML<SerializedScoreList>();
            Assert.That(scores.Scores, Has.Count.EqualTo(1));
        }

        // Check for all versus score types
        for (byte i = 5; i <= 7; i++)
        {
            HttpResponseMessage message = client.GetAsync($"/lbp/topscores/user/{level.LevelId}/{i}").Result;
            Assert.That(message.StatusCode, Is.EqualTo(OK));
            
            SerializedScoreList scores = message.Content.ReadAsXML<SerializedScoreList>();
            Assert.That(scores.Scores, Has.Count.EqualTo(4));
        }
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
        GameScore score = context.SubmitScore(submittedScore, 1, level, user, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);

        List<ObjectId> scores = context.Database.GetRankedScoresAroundScore(score, count)!
            .Items
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
        
        GameScore score1 = context.SubmitScore(0, 1, level, user1, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        GameScore score2 = context.SubmitScore(0, 2, level, user2, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        
        Assert.Multiple(() =>
        {
            Assert.That(context.Database.GetTopScoresForLevel(level, 10, 0, 1).Items.Select(s => s.score), Does.Not.Contain(score2));
            Assert.That(context.Database.GetTopScoresForLevel(level, 10, 0, 2).Items.Select(s => s.score), Does.Not.Contain(score1));
            Assert.That(context.Database.GetTopScoresForLevel(level, 10, 0, 0).Items.Count(), Is.EqualTo(2));
        });
    }

    [Test]
    public void FailsWithInvalidNumber()
    {
        using TestContext context = this.GetServer(false);
        GameUser user = context.CreateUser();
        GameScore score = context.SubmitScore(0, 1, context.CreateLevel(user), user, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        
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

        Assert.That(context.Database.GetTotalPlaysForLevel(level), Is.EqualTo(1));
    }
    
    [Test]
    public void PlayLevelMultipleTimes()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        for (int i = 0; i < 5; i++)
        {
            HttpResponseMessage message = client.PostAsync($"/lbp/play/user/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
            Assert.That(message.StatusCode, Is.EqualTo(OK));

            context.Database.Refresh();

            Assert.That(context.Database.GetTotalPlaysForLevel(level), Is.EqualTo(i + 1));
        }
    }

    [Test]
    public void PlayUserLevelInLBP1()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        int plays = 2;

        for (int i = 0; i < plays; i++)
        {
            // Enter level (increment play count)
            HttpResponseMessage message = client.PostAsync($"/lbp/enterLevel/user/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
            Assert.That(message.StatusCode, Is.EqualTo(OK));

            // Enter level again (no increment)
            message = client.PostAsync($"/lbp/enterLevel/user/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
            Assert.That(message.StatusCode, Is.EqualTo(OK));

            // Back to pod
            message = client.PostAsync($"/lbp/enterLevel/user/0", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
            Assert.That(message.StatusCode, Is.EqualTo(OK));
        }

        Assert.That(context.Database.HasUserPlayedLevel(level, user), Is.True);
        Assert.That(context.Database.GetTotalPlaysForLevelByUser(level, user), Is.EqualTo(plays));
    }

    [Test]
    public void PlayDeveloperLevelInLBP1()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        int plays = 2;

        for (int i = 0; i < plays; i++)
        {
            // Enter level (don't increment play count)
            HttpResponseMessage message = client.PostAsync($"/lbp/enterLevel/developer/1", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
            Assert.That(message.StatusCode, Is.EqualTo(OK));

            // Play level (do increment)
            message = client.PostAsync($"/lbp/play/developer/1", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
            Assert.That(message.StatusCode, Is.EqualTo(OK));

            // Back to pod
            message = client.PostAsync($"/lbp/enterLevel/user/0", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
            Assert.That(message.StatusCode, Is.EqualTo(OK));
        }

        GameLevel level = context.Database.GetStoryLevelById(1);

        Assert.That(context.Database.HasUserPlayedLevel(level, user), Is.True);
        Assert.That(context.Database.GetTotalPlaysForLevelByUser(level, user), Is.EqualTo(plays));
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

        Assert.That(context.Database.GetTotalPlaysForLevel(level), Is.EqualTo(2));
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

    [Test]
    public void OvertakeNotificationsWorkProperly()
    {
        using TestContext context = this.GetServer();
        const int userAmount = 10;

        List<GameUser> users = new(userAmount);
        for (int i = 0; i < userAmount; i++)
        {
            users.Add(context.CreateUser());
        }
        
        GameLevel level = context.CreateLevel(users.First());

        bool testedSent = false;
        bool testedNotSent = false;

        for (int i = 0; i < users.Count; i++)
        {
            GameScore? lastBestScore = context.Database.GetTopScoresForLevel(level, 1, 0, 1, true).Items.FirstOrDefault()?.score;
            
            GameUser user = users[i];
            context.SubmitScore(i, 1, level, user, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);

            // Check that notification was sent to the last #1 users
            if (lastBestScore == null) continue;

            GameUser notificationRecipient = context.Database.GetSubmittingPlayerFromScore(lastBestScore)!;
            GameNotification? notification = context.Database.GetNotificationsByUser(notificationRecipient, 1, 0).Items.FirstOrDefault();

            if (lastBestScore.Score <= 0)
            {
                Assert.That(notification, Is.Null);
                testedNotSent = true;
            }
            else
            {
                Assert.That(notification, Is.Not.Null);
                context.Database.DeleteNotification(notification!);
                testedSent = true;
            }

            // Check that notification was not sent to people who weren't previously #1
            if (context.Database.GetTotalPlaysForLevel(level) <= 2) continue;
            
            notificationRecipient = users[i - 2];
            Assert.That(context.Database.GetNotificationCountByUser(notificationRecipient), Is.Zero);
        }
        
        Assert.That(testedSent && testedNotSent, Is.True);
    }

    [Test]
    public void DontSpamSecondBestPlayerWithOvertakeNotifs()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user1);

        const int overtakeAmount = 5;
        context.SubmitScore(10, 1, level, user2, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);

        for (int i = 0; i < overtakeAmount; i++)
        {
            context.SubmitScore(20 * i + 20, 1, level, user1, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);

            // Make sure that user1 is always #1
            Assert.That(context.Database.GetTopScoresForLevel(level, 1, 0, 1, true).Items.FirstOrDefault()?.score.PlayerIds[0], Is.EqualTo(user1.UserId));
        }

        // Make sure #2 only has one overtake notif
        Assert.That(context.Database.GetNotificationCountByUser(user2), Is.EqualTo(1));
    }

    [Test]
    public void OnlySendOvertakeNotifsToRelevantPlayers()
    {
        using TestContext context = this.GetServer();
        GameUser both1 = context.CreateUser();
        GameUser both2 = context.CreateUser();
        GameUser first3 = context.CreateUser();
        GameUser first4 = context.CreateUser();
        GameUser second3 = context.CreateUser();
        GameUser second4 = context.CreateUser();
        GameLevel level = context.CreateLevel(both1);

        List<GameUser> lobby1 = [both1, both2, first3, first4];
        GameScore score1 = context.SubmitScore(10, 4, level, both1, TokenGame.LittleBigPlanet2, TokenPlatform.PS3, lobby1);

        // Now overtake with half the players being new (also shuffled order)
        List<GameUser> lobby2 = [both2, second3, second4, both1];
        GameScore score2 = context.SubmitScore(123456, 4, level, both2, TokenGame.LittleBigPlanet2, TokenPlatform.PS3, lobby2);

        Assert.That(score1.PlayerIds.All(p => lobby1.Select(u => u.UserId).Contains(p)));
        Assert.That(score2.PlayerIds.All(p => lobby2.Select(u => u.UserId).Contains(p)));

        // Make sure that both1 and both2 have received a notification, but the others have not. 
        // Also make sure there are now 2 scores on the leaderboard.
        IEnumerable<ScoreWithRank> scores = context.Database.GetTopScoresForLevel(level, 100, 0, 4, false).Items;
        Assert.That(scores.Count(), Is.EqualTo(2));
        Assert.That(scores.Any(s => s.score.ScoreId == score1.ScoreId));
        Assert.That(scores.Any(s => s.score.ScoreId == score2.ScoreId));

        Assert.That(context.Database.GetNotificationCountByUser(both1), Is.Zero);
        Assert.That(context.Database.GetNotificationCountByUser(both2), Is.Zero);
        Assert.That(context.Database.GetNotificationCountByUser(first3), Is.EqualTo(1));
        Assert.That(context.Database.GetNotificationCountByUser(first4), Is.EqualTo(1));
        Assert.That(context.Database.GetNotificationCountByUser(second3), Is.Zero);
        Assert.That(context.Database.GetNotificationCountByUser(second4), Is.Zero);
    }

    [Test]
    public async Task GamePaginationSortsCorrectly()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);
        
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        
        context.FillLeaderboard(level, 4, 1);

        HttpResponseMessage response = await client.GetAsync($"/lbp/topscores/user/{level.LevelId}/1?pageStart=1&pageSize=2");
        SerializedScoreList firstPage = response.Content.ReadAsXML<SerializedScoreList>();
        
        Assert.Multiple(() =>
        {
            Assert.That(firstPage.Scores, Has.Count.EqualTo(2));
            Assert.That(firstPage.Scores[0].Rank, Is.EqualTo(1));
            Assert.That(firstPage.Scores[1].Rank, Is.EqualTo(2));
        });
        
        response = await client.GetAsync($"/lbp/topscores/user/{level.LevelId}/1?pageStart=3&pageSize=2");
        SerializedScoreList secondPage = response.Content.ReadAsXML<SerializedScoreList>();
        
        Assert.Multiple(() =>
        {
            Assert.That(secondPage.Scores, Has.Count.EqualTo(2));
            Assert.That(secondPage.Scores[0].Rank, Is.EqualTo(3));
            Assert.That(secondPage.Scores[1].Rank, Is.EqualTo(4)); 
        });
    }

    [Test]
    [TestCase(1)]
    [TestCase(4)]
    public void ConvertScoreType7(byte playerCount)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        SerializedScore score = new()
        {
            Host = true,
            ScoreType = 7,
            Score = 5,
            PlayerUsernames = [user.Username],
        };

        // Start at 1 to skip publisher, who is already in the list
        for (int i = 1; i < playerCount; i++) {
            score.PlayerUsernames.Add(context.CreateUser($"FRIEND{i}").Username);
        }

        HttpResponseMessage message = client.PostAsync($"/lbp/play/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        message = client.PostAsync($"/lbp/scoreboard/user/{level.LevelId}", new StringContent(score.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Make sure the score is under the converted score type
        Assert.That(context.Database.GetTopScoresForLevel(level, 100, 0, playerCount, false).TotalItems, Is.EqualTo(1));

        // Make sure there are no actual type 7 scores
        Assert.That(context.Database.GetTopScoresForLevel(level, 100, 0, 7, false).TotalItems, Is.Zero);
    }
}