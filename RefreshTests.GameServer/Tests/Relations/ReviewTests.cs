using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using RefreshTests.GameServer.Extensions;
using Refresh.Database.Models.Levels;
using Refresh.Interfaces.Game.Types.Reviews;

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
        
        using HttpClient reviewerClient = context.GetAuthenticatedClient(TokenType.Game, reviewPublisher);

        GameLevel level = slotType == "developer" ? context.Database.GetStoryLevelById(1) : context.CreateLevel(levelPublisher);

        int levelId = slotType == "developer" ? 1 : level.LevelId;
        
        context.Database.PlayLevel(level, reviewPublisher, 1);
        context.Database.Refresh();

        SerializedGameReview review = new()
        {
            RawLabels = "LABEL_SurvivalChallenge",
            Content = "moku Sutolokanopu",
        };
        
        Assert.That(reviewerClient.PostAsync($"/lbp/postReview/{slotType}/{levelId}", new StringContent(review.AsXML())).Result.StatusCode, Is.EqualTo(OK));

        HttpResponseMessage response = reviewerClient.GetAsync($"/lbp/reviewsFor/{slotType}/{levelId}").Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));

        SerializedGameReviewResponse levelReviews = response.Content.ReadAsXML<SerializedGameReviewResponse>();
        Assert.That(levelReviews.Items, Has.Count.EqualTo(1));
        Assert.That(levelReviews.Items[0].Content, Is.EqualTo(review.Content));
        Assert.That(levelReviews.Items[0].Labels, Is.EqualTo(review.Labels));
        
        response = reviewerClient.GetAsync($"/lbp/reviewsBy/{reviewPublisher.Username}").Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));

        levelReviews = response.Content.ReadAsXML<SerializedGameReviewResponse>();
        Assert.That(levelReviews.Items, Has.Count.EqualTo(1));
        Assert.That(levelReviews.Items[0].Content, Is.EqualTo(review.Content));
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
        
        Assert.That(client.GetAsync($"/lbp/reviewsFor/badType/{level.LevelId}").Result.StatusCode, Is.EqualTo(NotFound));
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

        using HttpClient reviewerClient = context.GetAuthenticatedClient(TokenType.Game, reviewPublisher);

        GameLevel level = context.CreateLevel(levelPublisher);

        context.Database.PlayLevel(level, reviewPublisher, 1);

        SerializedGameReview review = new()
        {
            RawLabels = "LABEL_SurvivalChallenge",
            Content = "moku Sutolokanopu",
        };
        
        Assert.That(reviewerClient.PostAsync($"/lbp/postReview/badType/{level.LevelId}", new StringContent(review.AsXML())).Result.StatusCode, Is.EqualTo(NotFound));
        
        context.Database.Refresh();
        Assert.That(context.Database.GetTotalReviewsForLevel(level), Is.Zero);
    }
    
    [Test]
    public void CantPostReviewOnInvalidLevel()
    {
        using TestContext context = this.GetServer();
        
        GameUser reviewPublisher = context.CreateUser();

        using HttpClient reviewerClient = context.GetAuthenticatedClient(TokenType.Game, reviewPublisher);

        SerializedGameReview review = new()
        {
            RawLabels = "LABEL_SurvivalChallenge",
            Content = "moku Sutolokanopu",
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
            RawLabels = "LABEL_SurvivalChallenge",
            Content = "moku Sutolokanopu",
        };
        
        Assert.That(reviewerClient.PostAsync($"/lbp/postReview/user/{level.LevelId}", new StringContent(review.AsXML())).Result.StatusCode, Is.EqualTo(BadRequest));
        
        context.Database.Refresh();
        Assert.That(context.Database.GetTotalReviewsForLevel(level), Is.Zero);
    }
}