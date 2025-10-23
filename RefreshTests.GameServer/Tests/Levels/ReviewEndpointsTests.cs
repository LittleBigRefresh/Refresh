using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;
using Refresh.Interfaces.Game.Types.Reviews;
using RefreshTests.GameServer.Extensions;
using Refresh.Common.Constants;

namespace RefreshTests.GameServer.Tests.Levels;

public class ReviewEndpointsTests : GameServerTest
{
    [Test]
    public void DpadRateLevelBoo()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/dpadrate/user/{level.LevelId}?rating={(sbyte)RatingType.Boo}", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        context.Database.Refresh();
        Assert.That(context.Database.GetRatingByUser(level, user1), Is.EqualTo(RatingType.Boo));
        Assert.That(context.Database.GetRatingByUser(level, user2), Is.EqualTo(null));
    } 
    
    [Test]
    public void DpadRateLevelNeutral()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/dpadrate/user/{level.LevelId}?rating={(sbyte)RatingType.Neutral}", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        context.Database.Refresh();
        Assert.That(context.Database.GetRatingByUser(level, user1), Is.EqualTo(RatingType.Neutral));
        Assert.That(context.Database.GetRatingByUser(level, user2), Is.EqualTo(null));
    } 
    
    [Test]
    public void DpadRateLevelYay()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/dpadrate/user/{level.LevelId}?rating={(sbyte)RatingType.Yay}", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        context.Database.Refresh();
        Assert.That(context.Database.GetRatingByUser(level, user1), Is.EqualTo(RatingType.Yay));
        Assert.That(context.Database.GetRatingByUser(level, user2), Is.EqualTo(null));
    } 
    
    [Test]
    public void DpadRateLevelOverwrite()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/dpadrate/user/{level.LevelId}?rating={(sbyte)RatingType.Yay}", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        context.Database.Refresh();
        Assert.That(context.Database.GetRatingByUser(level, user1), Is.EqualTo(RatingType.Yay));
        Assert.That(context.Database.GetRatingByUser(level, user2), Is.EqualTo(null));
        
        message = client.PostAsync($"/lbp/dpadrate/user/{level.LevelId}?rating={(sbyte)RatingType.Neutral}", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        context.Database.Refresh();
        Assert.That(context.Database.GetRatingByUser(level, user1), Is.EqualTo(RatingType.Neutral));
        Assert.That(context.Database.GetRatingByUser(level, user2), Is.EqualTo(null));
    } 
    
    [Test]
    public void CantDpadRateLevelWithInvalidRating()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        context.Database.PlayLevel(level, user1, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/dpadrate/user/{level.LevelId}?rating=YAY", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
        
        message = client.PostAsync($"/lbp/dpadrate/user/{level.LevelId}?rating=2", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    } 

    [Test]
    public void CantDpadRateInvalidLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message = client.PostAsync($"/lbp/dpadrate/user/{int.MaxValue}?rating=1", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    } 
    
    [Test]
    public void RateLevel1Star()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/rate/user/{level.LevelId}?rating=1", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        context.Database.Refresh();
        Assert.That(context.Database.GetRatingByUser(level, user1), Is.EqualTo(RatingType.Boo));
        Assert.That(context.Database.GetRatingByUser(level, user2), Is.EqualTo(null));
    } 
    
    [Test]
    public void RateLevel2Star()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/rate/user/{level.LevelId}?rating=2", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        context.Database.Refresh();
        Assert.That(context.Database.GetRatingByUser(level, user1), Is.EqualTo(RatingType.Boo));
        Assert.That(context.Database.GetRatingByUser(level, user2), Is.EqualTo(null));
    } 
    
    [Test]
    public void RateLevel3Star()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/rate/user/{level.LevelId}?rating=3", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        context.Database.Refresh();
        Assert.That(context.Database.GetRatingByUser(level, user1), Is.EqualTo(RatingType.Neutral));
        Assert.That(context.Database.GetRatingByUser(level, user2), Is.EqualTo(null));
    } 
    
    [Test]
    public void RateLevel4Star()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/rate/user/{level.LevelId}?rating=4", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        context.Database.Refresh();
        Assert.That(context.Database.GetRatingByUser(level, user1), Is.EqualTo(RatingType.Yay));
        Assert.That(context.Database.GetRatingByUser(level, user2), Is.EqualTo(null));
    } 
    
    [Test]
    public void RateLevel5Star()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/rate/user/{level.LevelId}?rating=5", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        context.Database.Refresh();
        Assert.That(context.Database.GetRatingByUser(level, user1), Is.EqualTo(RatingType.Yay));
        Assert.That(context.Database.GetRatingByUser(level, user2), Is.EqualTo(null));
    } 
    
    [Test]
    public void CantRateInvalidLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message = client.PostAsync($"/lbp/rate/user/{int.MaxValue}?rating=1", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    } 
    
    [Test]
    public void CantRateLevelWithInvalidRating()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/rate/user/{level.LevelId}?rating=WHAT", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
        
        message = client.PostAsync($"/lbp/rate/user/{level.LevelId}?rating=6", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    } 

    [Test]
    public void CanReviewPlayedLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);

        List<Label> labels = [ Label.Short, Label.BouncePads, Label.Water ];
        SerializedGameReview review = new()
        {
            Content = "Sucks",
            RawLabels = LabelExtensions.ToLbpCommaList(labels),
        };

        HttpResponseMessage message = client.PostAsync($"/lbp/postReview/user/{level.LevelId}", new StringContent(review.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        GameReview? postedReview = context.Database.GetReviewByLevelAndUser(level, user1);
        Assert.That(postedReview, Is.Not.Null);
        Assert.That(postedReview!.Content, Is.EqualTo("Sucks"));
        Assert.That(postedReview!.Labels, Is.EqualTo(labels));
    }

    [Test]
    public void ReviewContentGetsTrimmed()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);

        SerializedGameReview review = new()
        {
            Content = new string('S', 9000),
        };

        HttpResponseMessage message = client.PostAsync($"/lbp/postReview/user/{level.LevelId}", new StringContent(review.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        GameReview? postedReview = context.Database.GetReviewByLevelAndUser(level, user1);
        Assert.That(postedReview, Is.Not.Null);
        Assert.That(postedReview!.Content.Length, Is.EqualTo(UgcLimits.CommentLimit));
    }

    [Test]
    public void SanitizeReviewLabelsIfDuplicates()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);

        List<Label> labels = [ Label.Water, Label.Water, Label.Water ];
        SerializedGameReview review = new()
        {
            Content = "Sucks",
            RawLabels = LabelExtensions.ToLbpCommaList(labels),
        };

        HttpResponseMessage message = client.PostAsync($"/lbp/postReview/user/{level.LevelId}", new StringContent(review.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        GameReview? postedReview = context.Database.GetReviewByLevelAndUser(level, user1);
        Assert.That(postedReview, Is.Not.Null);
        Assert.That(postedReview!.Labels, Is.EqualTo([ Label.Water ]));
    }

    [Test]
    public void SanitizeReviewLabelsIfTooMany()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);

        List<Label> labels = 
        [ 
            Label.Water, Label.ArcadeGame, 
            Label.Sackbots, Label.Artistic, 
            Label.Intricate, Label.Collectables, 
            Label.Swoop 
        ];
        SerializedGameReview review = new()
        {
            Content = "Sucks",
            RawLabels = LabelExtensions.ToLbpCommaList(labels),
        };

        HttpResponseMessage message = client.PostAsync($"/lbp/postReview/user/{level.LevelId}", new StringContent(review.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        GameReview? postedReview = context.Database.GetReviewByLevelAndUser(level, user1);
        Assert.That(postedReview, Is.Not.Null);
        Assert.That(postedReview!.Labels, Is.EqualTo(labels.Take(UgcLimits.MaximumLabels).ToList()));
    }

    [Test]
    public void TestRecurringLevelLabels()
    {
        using TestContext context = this.GetServer();
        
        GameUser publisher = context.CreateUser();
        GameUser reviewer1 = context.CreateUser();
        GameUser reviewer2 = context.CreateUser();
        GameUser reviewer3 = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);

        // Post a few reviews with different certain labels
        context.Database.AddReviewToLevel(new SerializedGameReview()
        {
            Content = "Hi",
            Labels = [Label.Water],
        }, level, reviewer1);

        context.Database.AddReviewToLevel(new SerializedGameReview()
        {
            Content = "Water Cooler",
            Labels = [Label.Water, Label.ArcadeGame],
        }, level, reviewer2);

        context.Database.AddReviewToLevel(new SerializedGameReview()
        {
            Content = "GG ez",
            Labels = [Label.Water, Label.Easy, Label.ArcadeGame],
        }, level, reviewer3);

        // Recalculate stats for the level
        context.Database.RecalculateLevelStatistics(level);
        Assert.That(level.Statistics, Is.Not.Null);
        Assert.That(level.Statistics!.RecurringLabels, Is.Not.Empty);

        // Compare labels
        List<Label> actualLabels = level.Statistics!.RecurringLabels;
        List<Label> expectedLabels = [Label.Water, Label.ArcadeGame, Label.Easy];
        Assert.That(actualLabels, Is.EqualTo(expectedLabels));
    } 
}