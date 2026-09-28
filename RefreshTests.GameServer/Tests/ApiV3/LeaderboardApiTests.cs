using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Levels;
using Refresh.Interfaces.Game.Types.UserData.Leaderboard;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.ApiV3;

public class LeaderboardApiTests : GameServerTest
{
    [Test]
    public async Task DeletesScoresByPublisherUuidAndName()
    {
        using TestContext context = this.GetServer();
        GameUser mod = context.CreateUser(role: GameUserRole.Moderator);
        GameLevel level = context.CreateLevel(mod);
        GameUser publisher = context.CreateUser(role: GameUserRole.User);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, mod);

        // UUID
        context.SubmitScore(4000001, 1, level, publisher, TokenGame.LittleBigPlanet1, TokenPlatform.RPCS3, [publisher]);
        Assert.That(context.Database.GetTopScoresForLevel(level, 100, 0, 1).TotalItems, Is.EqualTo(1)); // TODO: Use total score by publisher
        HttpResponseMessage resetResponse = await client.DeleteAsync($"/api/v3/admin/users/uuid/{publisher.UserId}/scores");
        Assert.That(resetResponse.IsSuccessStatusCode, Is.True);
        Assert.That(context.Database.GetTopScoresForLevel(level, 100, 0, 1).TotalItems, Is.Zero);

        // name
        context.SubmitScore(4000001, 1, level, publisher, TokenGame.LittleBigPlanet1, TokenPlatform.RPCS3, [publisher]);
        Assert.That(context.Database.GetTopScoresForLevel(level, 100, 0, 1).TotalItems, Is.EqualTo(1));
        resetResponse = await client.DeleteAsync($"/api/v3/admin/users/name/{publisher.Username}/scores");
        Assert.That(resetResponse.IsSuccessStatusCode, Is.True);
        Assert.That(context.Database.GetTopScoresForLevel(level, 100, 0, 1).TotalItems, Is.Zero);
    }

    private void AssertResponseListCount(ApiListResponse<ApiGameScoreResponse>? response, int count)
    {
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response.ListInfo, Is.Not.Null);
        Assert.That(response.Data.Count, Is.EqualTo(count));
        Assert.That(response.ListInfo.TotalItems, Is.EqualTo(count));
    }
    
    [Test]
    public void GetsScoresByUserOnVariousLevels()
    {
        using TestContext context = this.GetServer();
        GameUser uploader = context.CreateUser();
        
        // Post 1 score per mode per level, 3 levels in total, each score is slightly higher than the previous one.
        for (int i = 0; i < 3; i++)
        {
            GameLevel level = context.CreateLevel(uploader);
            for (byte mode = 1; mode <= 4; mode++)
            {
                SerializedScore score = new()
                {
                    Host = true,
                    ScoreType = mode,
                    Score = 434834,
                    PlayerUsernames = [uploader.Username],
                };
                context.Database.SubmitScore(score, uploader, level, TokenGame.LittleBigPlanet1, TokenPlatform.RPCS3, [uploader]);
            }
        }

        for (byte mode = 1; mode <= 4; mode++)
        {
            // UUID
            ApiListResponse<ApiGameScoreResponse>? response = context.Http.GetList<ApiGameScoreResponse>($"/api/v3/users/uuid/{uploader.UserId.ToString()}/scores?mode={mode}");
            this.AssertResponseListCount(response, 3);
            
            // name
            response = context.Http.GetList<ApiGameScoreResponse>($"/api/v3/users/name/{uploader.Username}/scores?mode={mode}");
            this.AssertResponseListCount(response, 3);
        }
        
        // Ensure that no mode and mode 0 will both return all scores.
        ApiListResponse<ApiGameScoreResponse>? bigResponse = context.Http.GetList<ApiGameScoreResponse>($"/api/v3/users/name/{uploader.Username}/scores");
        this.AssertResponseListCount(bigResponse, 12);
        
        bigResponse = context.Http.GetList<ApiGameScoreResponse>($"/api/v3/users/name/{uploader.Username}/scores?mode=0");
        this.AssertResponseListCount(bigResponse, 12);
    }
    
    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public void GetsScoresByUserOnSameLevel(bool showOvertaken)
    {
        using TestContext context = this.GetServer();
        GameUser uploader = context.CreateUser();
        GameLevel level = context.CreateLevel(uploader);
        
        // Post 3 scores per mode to the same level, each score is slightly higher than the previous one,
        // so the user will end up overtaking their own score.
        for (int i = 0; i < 3; i++)
        {
            for (byte mode = 1; mode <= 4; mode++)
            {
                SerializedScore score = new()
                {
                    Host = true,
                    ScoreType = mode,
                    Score = 434834 + i,
                    PlayerUsernames = [uploader.Username],
                };
                context.Database.SubmitScore(score, uploader, level, TokenGame.LittleBigPlanet1, TokenPlatform.RPCS3, [uploader]);
            }
        }

        for (byte mode = 1; mode <= 4; mode++)
        {
            // Show either just the one best score on this level, or also include the 2 overtaken ones.
            // UUID
            ApiListResponse<ApiGameScoreResponse>? response = context.Http.GetList<ApiGameScoreResponse>($"/api/v3/users/uuid/{uploader.UserId.ToString()}/scores?mode={mode}&showAll={showOvertaken}");
            this.AssertResponseListCount(response,  showOvertaken ? 3 : 1);
            
            // name
            response = context.Http.GetList<ApiGameScoreResponse>($"/api/v3/users/name/{uploader.Username}/scores?mode={mode}&showAll={showOvertaken}");
            this.AssertResponseListCount(response, showOvertaken ? 3 : 1);
        }
        
        // Ensure that no mode and mode 0 will both return all scores.
        // Will either return just the best score per mode, or all of them.
        ApiListResponse<ApiGameScoreResponse>? bigResponse = context.Http.GetList<ApiGameScoreResponse>($"/api/v3/users/name/{uploader.Username}/scores?showAll={showOvertaken}");
        this.AssertResponseListCount(bigResponse, showOvertaken ? 12 : 4);
        
        bigResponse = context.Http.GetList<ApiGameScoreResponse>($"/api/v3/users/name/{uploader.Username}/scores?mode=0&showAll={showOvertaken}");
        this.AssertResponseListCount(bigResponse, showOvertaken ? 12 : 4);
    }
    
    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public void DontFilterOutScoresNotOvertakenByOwnPublisher(bool showOvertaken)
    {
        using TestContext context = this.GetServer();
        GameUser uploader = context.CreateUser();
        GameLevel level = context.CreateLevel(uploader);

        // Upload the scores of the user we want, one per type
        for (byte mode = 1; mode <= 4; mode++)
        {
            SerializedScore ogScore = new()
            {
                Host = true,
                ScoreType = mode,
                Score = 2344,
                PlayerUsernames = [uploader.Username],
            };
            context.Database.SubmitScore(ogScore, uploader, level, TokenGame.LittleBigPlanet1, TokenPlatform.RPCS3, [uploader]);
        }

        // Post 1 score per extra user per mode to the same level, each score is slightly higher than the previous one,
        // so we will have 4 scores by a different user each per mode.
        for (int i = 0; i < 2; i++)
        {
            GameUser anotherUploader = context.CreateUser();
            for (byte mode = 1; mode <= 4; mode++)
            {
                SerializedScore score = new()
                {
                    Host = true,
                    ScoreType = mode,
                    Score = 434834 + i,
                    PlayerUsernames = [anotherUploader.Username],
                };
                context.Database.SubmitScore(score, anotherUploader, level, TokenGame.LittleBigPlanet1, TokenPlatform.RPCS3, [anotherUploader]);
            }
        }

        for (byte mode = 1; mode <= 4; mode++)
        {
            // Show either just the one best score on this level, or also include the 2 overtaken ones.
            // UUID
            ApiListResponse<ApiGameScoreResponse>? response = context.Http.GetList<ApiGameScoreResponse>($"/api/v3/users/uuid/{uploader.UserId.ToString()}/scores?mode={mode}&showAll={showOvertaken}");
            this.AssertResponseListCount(response,  1); // user still has just one of this type, regardless of whether we're showing all scores by them
            
            response = context.Http.GetList<ApiGameScoreResponse>($"/api/v3/scores/{level.LevelId}/{mode}?showAll={showOvertaken}");
            this.AssertResponseListCount(response,  3); // the user's score + the ones by the 2 extra users
        }
        
        ApiListResponse<ApiGameScoreResponse>? bigResponse = context.Http.GetList<ApiGameScoreResponse>($"/api/v3/users/uuid/{uploader.UserId.ToString()}/scores?showAll={showOvertaken}");
        this.AssertResponseListCount(bigResponse,  4); // user still has just one per type, and they're all not overtaken by themselves
            
        bigResponse = context.Http.GetList<ApiGameScoreResponse>($"/api/v3/scores/{level.LevelId}/0?showAll={showOvertaken}");
        this.AssertResponseListCount(bigResponse,  12); // all scores by all users regardless of type
    }
    
    [Test]
    public void CanFilterUserScoresByRank()
    {
        using TestContext context = this.GetServer();
        GameUser uploader = context.CreateUser();

        // Upload scores by the same user on various levels
        for (int i = 0; i < 10; i++)
        {
            GameLevel anotherLevel = context.CreateLevel(uploader);
            SerializedScore score = new()
            {
                Host = true,
                ScoreType = 1,
                Score = i,
                PlayerUsernames = [uploader.Username],
            };
            context.Database.SubmitScore(score, uploader, anotherLevel, TokenGame.LittleBigPlanet1, TokenPlatform.RPCS3, [uploader]);
            
            // Also put scores by others there, so the uploader has a score per level, and these scores all have different ranks
            for (int u = 0; u < 10; u++)
            {
                GameUser anotherUploader = context.CreateUser();
                SerializedScore anotherScore = new()
                {
                    Host = true,
                    ScoreType = 1,
                    Score = u,
                    PlayerUsernames = [anotherUploader.Username],
                };
                context.Database.SubmitScore(anotherScore, anotherUploader, anotherLevel, TokenGame.LittleBigPlanet1, TokenPlatform.RPCS3, [anotherUploader]);
            }
        }
        
        context.Database.Refresh();
        
        // No filters
        ApiListResponse<ApiGameScoreResponse>? bigResponse = context.Http.GetList<ApiGameScoreResponse>($"/api/v3/users/uuid/{uploader.UserId.ToString()}/scores");
        this.AssertResponseListCount(bigResponse, 10);
        
        // Settings filters to 0 is the same as no filter
        bigResponse = context.Http.GetList<ApiGameScoreResponse>($"/api/v3/users/uuid/{uploader.UserId.ToString()}/scores?minRank=0&maxRank=0");
        this.AssertResponseListCount(bigResponse, 10);

        // Can use just minRank
        bigResponse = context.Http.GetList<ApiGameScoreResponse>($"/api/v3/users/uuid/{uploader.UserId.ToString()}/scores?minRank=5");
        this.AssertResponseListCount(bigResponse, 6);

        // Can use just maxRank
        bigResponse = context.Http.GetList<ApiGameScoreResponse>($"/api/v3/users/uuid/{uploader.UserId.ToString()}/scores?maxRank=3");
        this.AssertResponseListCount(bigResponse, 3);

        // Can use both
        bigResponse = context.Http.GetList<ApiGameScoreResponse>($"/api/v3/users/uuid/{uploader.UserId.ToString()}/scores?minRank=4&maxRank=7");
        this.AssertResponseListCount(bigResponse, 4);

        // Can use both to return all scores of just one rank
        bigResponse = context.Http.GetList<ApiGameScoreResponse>($"/api/v3/users/uuid/{uploader.UserId.ToString()}/scores?minRank=1&maxRank=1");
        this.AssertResponseListCount(bigResponse, 1);
    }
}