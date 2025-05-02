using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Database.Models.Levels;

namespace RefreshTests.GameServer.Tests.Levels;

public class ScoreModerationTests : GameServerTest
{
    [Test]
    public void DeletesIndividualScore()
    {
        // Arrange
        using TestContext context = this.GetServer();
        
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);
        
        GameSubmittedScore score = context.SubmitScore(1, 1, level, user, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        string uuid = score.ScoreId.ToString();
        
        // Act
        context.Database.DeleteScore(context.Database.GetScoreByUuid(uuid)!);
        
        // Assert
        Assert.That(context.Database.GetScoreByUuid(uuid), Is.Null);
    }
    
    [Test]
    public async Task AnonymousCannotDeleteIndividualScoreViaApi()
    {
        // Arrange
        using TestContext context = this.GetServer();
        
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);
        
        GameSubmittedScore score = context.SubmitScore(1, 1, level, user, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        string uuid = score.ScoreId.ToString();
        
        // Act
        HttpResponseMessage response = await context.Http.DeleteAsync($"/api/v3/admin/scores/{uuid}");
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(Forbidden));
        Assert.That(context.Database.GetScoreByUuid(uuid), Is.Not.Null);
    }
    
    [Test]
    public async Task DeletesIndividualScoreViaApi()
    {
        // Arrange
        using TestContext context = this.GetServer();
        
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);
        context.Database.SetUserRole(user, GameUserRole.Admin);
        
        GameSubmittedScore score = context.SubmitScore(1, 1, level, user, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        string uuid = score.ScoreId.ToString();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);
        
        // Act
        HttpResponseMessage response = await client.DeleteAsync($"/api/v3/admin/scores/{uuid}");
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(OK));
        Assert.That(context.Database.GetScoreByUuid(uuid), Is.Null);
    }
    
    [Test]
    public void DeletesAllScoresByUser()
    {
        // Arrange
        using TestContext context = this.GetServer();
        
        GameUser user = context.CreateUser();
        GameLevel level1 = context.CreateLevel(user);
        GameLevel level2 = context.CreateLevel(user);
        
        GameSubmittedScore score1 = context.SubmitScore(1, 1, level1, user, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        GameSubmittedScore score2 = context.SubmitScore(1, 1, level2, user, TokenGame.LittleBigPlanet2, TokenPlatform.PS3);
        string uuid1 = score1.ScoreId.ToString();
        string uuid2 = score2.ScoreId.ToString();
        
        // Act
        context.Database.DeleteScoresSetByUser(user);
        
        // Assert
        Assert.That(context.Database.GetScoreByUuid(uuid1), Is.Null);
        Assert.That(context.Database.GetScoreByUuid(uuid2), Is.Null);
    }
}