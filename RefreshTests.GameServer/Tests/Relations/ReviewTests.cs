using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Reviews;
using Refresh.GameServer.Types.UserData;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Relations;

public class ReviewTests : GameServerTest
{
    [TestCase("user")]
    [TestCase("developer")]
    public void TestPostReview(string slotType)
    {
        using TestContext context = this.GetServer();
        
        GameUser levelPublisher = context.CreateUser();
        GameUser reviewPublisher = context.CreateUser();

        GameLevel level = slotType == "developer" ? context.Database.GetStoryLevelById(1) : context.CreateLevel(levelPublisher);

        context.Database.PlayLevel(level, reviewPublisher, 1);
        context.Database.Refresh();
        
        using HttpClient reviewerClient = context.GetAuthenticatedClient(TokenType.Game, reviewPublisher);

        SerializedGameReview review = new()
        {
            Labels = "LABEL_SurvivalChallenge",
            Text = "moku Sutolokanopu",
        };
        
        Assert.That(reviewerClient.PostAsync($"/lbp/postReview/{slotType}/{level.LevelId}", new StringContent(review.AsXML())).Result.StatusCode, Is.EqualTo(OK));

        HttpResponseMessage response = reviewerClient.GetAsync($"/lbp/reviewsFor/{slotType}/{level.LevelId}").Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));

        SerializedGameReviewResponse levelReviews = response.Content.ReadAsXML<SerializedGameReviewResponse>();
        Assert.That(levelReviews.Items, Has.Count.EqualTo(1));
        Assert.That(levelReviews.Items[0].Text, Is.EqualTo(review.Text));
        Assert.That(levelReviews.Items[0].Labels, Is.EqualTo(review.Labels));
        
        response = reviewerClient.GetAsync($"/lbp/reviewsBy/{reviewPublisher.Username}").Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));

        levelReviews = response.Content.ReadAsXML<SerializedGameReviewResponse>();
        Assert.That(levelReviews.Items, Has.Count.EqualTo(1));
        Assert.That(levelReviews.Items[0].Text, Is.EqualTo(review.Text));
        Assert.That(levelReviews.Items[0].Labels, Is.EqualTo(review.Labels));
    }

    [Test]
    public void CantGetReviewsByInvalidUser()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        
        Assert.That(client.GetAsync("/lbp/reviewsBy/I_AM_NOT_REAL").Result.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void CantGetReviewsOnInvalidLevelSlotType()
    {
        using TestContext context = this.GetServer();
        
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);
        
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        
        Assert.That(client.GetAsync($"/lbp/reviewsFor/badType/{level.LevelId}").Result.StatusCode, Is.EqualTo(BadRequest));
    }
    
    [Test]
    public void CantGetReviewsOnInvalidLevelId()
    {
        using TestContext context = this.GetServer();
        
        GameUser user = context.CreateUser();
        
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        
        Assert.That(client.GetAsync($"/lbp/reviewsFor/user/{int.MaxValue}").Result.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void CantPostReviewOnInvalidSlotType()
    {
        using TestContext context = this.GetServer();
        
        GameUser levelPublisher = context.CreateUser();
        GameUser reviewPublisher = context.CreateUser();

        GameLevel level = context.CreateLevel(levelPublisher);

        context.Database.PlayLevel(level, reviewPublisher, 1);
        context.Database.Refresh();
        
        using HttpClient reviewerClient = context.GetAuthenticatedClient(TokenType.Game, reviewPublisher);

        SerializedGameReview review = new()
        {
            Labels = "LABEL_SurvivalChallenge",
            Text = "moku Sutolokanopu",
        };
        
        Assert.That(reviewerClient.PostAsync($"/lbp/postReview/badType/{level.LevelId}", new StringContent(review.AsXML())).Result.StatusCode, Is.EqualTo(BadRequest));
        
        context.Database.Refresh();
        Assert.That(level.Reviews, Is.Empty);
    }
    
    [Test]
    public void CantPostReviewOnInvalidLevel()
    {
        using TestContext context = this.GetServer();
        
        GameUser reviewPublisher = context.CreateUser();

        using HttpClient reviewerClient = context.GetAuthenticatedClient(TokenType.Game, reviewPublisher);

        SerializedGameReview review = new()
        {
            Labels = "LABEL_SurvivalChallenge",
            Text = "moku Sutolokanopu",
        };
        
        Assert.That(reviewerClient.PostAsync($"/lbp/postReview/user/{int.MaxValue}", new StringContent(review.AsXML())).Result.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void CantPostReviewOnUnplayedLevel()
    {
        using TestContext context = this.GetServer();
        
        GameUser levelPublisher = context.CreateUser();
        GameUser reviewPublisher = context.CreateUser();

        GameLevel level = context.CreateLevel(levelPublisher);

        using HttpClient reviewerClient = context.GetAuthenticatedClient(TokenType.Game, reviewPublisher);

        SerializedGameReview review = new()
        {
            Labels = "LABEL_SurvivalChallenge",
            Text = "moku Sutolokanopu",
        };
        
        Assert.That(reviewerClient.PostAsync($"/lbp/postReview/user/{level.LevelId}", new StringContent(review.AsXML())).Result.StatusCode, Is.EqualTo(BadRequest));
        
        context.Database.Refresh();
        Assert.That(level.Reviews, Is.Empty);
    }
}