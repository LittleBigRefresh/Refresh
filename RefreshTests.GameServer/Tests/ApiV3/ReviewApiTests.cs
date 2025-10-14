using Refresh.Common.Constants;
using Refresh.Database.Models;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Levels;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.ApiV3;

public class ReviewApiTests : GameServerTest
{
    [Test]
    public void PostAndUpdateReview()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);
        int id = level.LevelId;

        GameUser reviewer = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, reviewer);
        context.Database.PlayLevel(level, reviewer, 1);

        // Part 1: Initial review upload
        string initialContent = "This level stinks";
        ApiSubmitReviewRequest reviewToPost = new()
        {
            LevelRating = RatingType.Boo,
            Content = initialContent,
        };
        ApiResponse<ApiGameReviewResponse>? response = client.PostData<ApiGameReviewResponse>($"/api/v3/levels/id/{id}/reviews", reviewToPost);

        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Success, Is.True);
        Assert.That(response.Data!.Publisher.UserId, Is.EqualTo(reviewer.UserId.ToString()));
        Assert.That(response.Data!.Level.LevelId, Is.EqualTo(id));

        Assert.That(response.Data!.Text, Is.EqualTo(reviewToPost.Content));
        Assert.That(response.Data!.LevelRating, Is.EqualTo((int)RatingType.Boo));
        Assert.That(response.Data!.LabelList.Count, Is.EqualTo(0));

        // Part 2: Update the review by uploading a new one
        List<Label> labels = [Label.Short, Label.Tap];
        reviewToPost = new()
        {
            LevelRating = RatingType.Neutral,
            Labels = labels,
        };
        response = client.PostData<ApiGameReviewResponse>($"/api/v3/levels/id/{id}/reviews", reviewToPost);

        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Success, Is.True);
        Assert.That(response.Data!.Publisher.UserId, Is.EqualTo(reviewer.UserId.ToString()));
        Assert.That(response.Data!.Level.LevelId, Is.EqualTo(id));

        Assert.That(response.Data!.Text, Is.EqualTo(initialContent));
        Assert.That(response.Data!.LevelRating, Is.EqualTo((int)RatingType.Neutral));
        Assert.That(response.Data!.LabelList, Is.EqualTo(labels));

        // Make sure the reviewer doesn't have 2 reviews now
        Assert.That(context.Database.GetTotalReviewsForLevel(level), Is.EqualTo(1));
        Assert.That(context.Database.GetTotalReviewsByUser(reviewer), Is.EqualTo(1));

        // Part 3: Update by ID
        string newContent = "Actually idk this level might be interesting i forgor, stop booing me";
        reviewToPost = new()
        {
            Content = newContent
        };
        response = client.PatchData<ApiGameReviewResponse>($"/api/v3/reviews/id/{response.Data!.ReviewId}", reviewToPost);

        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Success, Is.True);
        Assert.That(response.Data!.Publisher.UserId, Is.EqualTo(reviewer.UserId.ToString()));
        Assert.That(response.Data!.Level.LevelId, Is.EqualTo(id));

        Assert.That(response.Data!.Text, Is.EqualTo(newContent));
        Assert.That(response.Data!.LevelRating, Is.EqualTo((int)RatingType.Neutral));
        Assert.That(response.Data!.LabelList, Is.EqualTo(labels));

        // Make sure the reviewer still doesn't have more than 1 review
        Assert.That(context.Database.GetTotalReviewsForLevel(level), Is.EqualTo(1));
        Assert.That(context.Database.GetTotalReviewsByUser(reviewer), Is.EqualTo(1));
    }

    [Test]
    public void CantEditNonexistentReview()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);
        int id = level.LevelId;

        GameUser reviewer = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, reviewer);
        context.Database.PlayLevel(level, reviewer, 1);

        ApiSubmitReviewRequest reviewToPost = new()
        {
            LevelRating = RatingType.Boo,
            Content = "I ran out of example text",
        };
        ApiResponse<ApiGameReviewResponse>? response = client.PatchData<ApiGameReviewResponse>($"/api/v3/reviews/id/420", reviewToPost, false);

        Assert.That(response?.Data, Is.Null);
        Assert.That(response!.Success, Is.False);
        Assert.That(response!.Error, Is.Not.Null);
        Assert.That(response.Error!.Name, Is.EqualTo("ApiNotFoundError"));
    }

    [Test]
    public void CantEditSomeoneElsesReview()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);
        int id = level.LevelId;

        GameUser reviewer = context.CreateUser();
        GameUser moron = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, moron);
        context.Database.PlayLevel(level, reviewer, 1);

        // Initially add a review and rating
        GameReview originalReview = context.Database.AddReviewToLevel(new ApiSubmitReviewRequest
        {
            Content = "Cool",
            Labels = [],
        }, level, reviewer);

        context.Database.RateLevel(level, reviewer, RatingType.Yay);

        // Try to edit them
        ApiSubmitReviewRequest reviewToPost = new()
        {
            LevelRating = RatingType.Boo,
            Labels = [Label.Scary],
            Content = "I'm the review ruiner, i will ruin your review",
        };
        ApiResponse<ApiGameReviewResponse>? response = client.PatchData<ApiGameReviewResponse>($"/api/v3/reviews/id/{originalReview.ReviewId}", reviewToPost, false);

        // Ensure the review wasn't touched
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Error, Is.Not.Null);
        Assert.That(response.Error!.Name, Is.EqualTo("ApiValidationError"));

        // Also test the 'get review by ID' endpoint while we're already at it
        response = client.GetData<ApiGameReviewResponse>($"/api/v3/reviews/id/{originalReview.ReviewId}");
        Assert.That(response!.Success, Is.True);
        Assert.That(response.Data!.Publisher.UserId, Is.EqualTo(reviewer.UserId.ToString()));
        Assert.That(response.Data!.Level.LevelId, Is.EqualTo(id));

        Assert.That(response.Data!.Text, Is.EqualTo(originalReview.Content));
        Assert.That(response.Data!.LevelRating, Is.EqualTo((int)RatingType.Yay));
        Assert.That(response.Data!.LabelList.Count, Is.EqualTo(0));
    }

    [Test]
    public void CantReviewOwnLevel()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);
        int id = level.LevelId;

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, publisher);
        context.Database.PlayLevel(level, publisher, 1);

        ApiSubmitReviewRequest reviewToPost = new()
        {
            LevelRating = RatingType.Yay,
            Labels = [Label.Long, Label.Funny, Label.Artistic],
            Content = "Wow what an amazing level, you should make more like this!",
        };
        ApiResponse<ApiGameReviewResponse>? response = client.PostData<ApiGameReviewResponse>($"/api/v3/levels/id/{id}/reviews", reviewToPost, false);

        // Ensure the review was rejected
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Error, Is.Not.EqualTo(null));
        Assert.That(response.Error!.Name, Is.EqualTo("ApiValidationError"));

        // Ensure there are no reviews
        Assert.That(context.Database.GetTotalReviewsForLevel(level), Is.EqualTo(0));
        Assert.That(context.Database.GetTotalReviewsByUser(publisher), Is.EqualTo(0));
        Assert.That(context.Database.GetRatingByUser(level, publisher), Is.Null);
    }

    [Test]
    public void RemoveDuplicateLabels()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);
        int id = level.LevelId;

        GameUser reviewer = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, reviewer);
        context.Database.PlayLevel(level, reviewer, 1);

        // Part 1: Initial review upload
        ApiSubmitReviewRequest reviewToPost = new()
        {
            LevelRating = RatingType.Yay,
            Labels = [Label.Water, Label.Water, Label.BoardGame, Label.Water],
            Content = "This stevel links",
        };
        ApiResponse<ApiGameReviewResponse>? response = client.PostData<ApiGameReviewResponse>($"/api/v3/levels/id/{id}/reviews", reviewToPost);

        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Success, Is.True);
        Assert.That(response.Data!.Publisher.UserId, Is.EqualTo(reviewer.UserId.ToString()));
        Assert.That(response.Data!.Level.LevelId, Is.EqualTo(id));

        Assert.That(response.Data!.Text, Is.EqualTo(reviewToPost.Content));
        Assert.That(response.Data!.LevelRating, Is.EqualTo((int)RatingType.Yay));
        Assert.That(response.Data!.LabelList.Count, Is.EqualTo(2));
        Assert.That(response.Data!.LabelList[0], Is.EqualTo(Label.Water));
        Assert.That(response.Data!.LabelList[1], Is.EqualTo(Label.BoardGame));
    }

    [Test]
    public void RemoveOverflowingLabels()
    {
        using TestContext context = this.GetServer();
        GameUser reviewer = context.CreateUser();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);
        int id = level.LevelId;

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, reviewer);
        context.Database.PlayLevel(level, reviewer, 1);

        List<Label> labels = 
        [ 
            Label.Water, Label.ArcadeGame, 
            Label.Sackbots, Label.Artistic, 
            Label.Intricate, Label.Collectables, 
            Label.Swoop 
        ];
        ApiSubmitReviewRequest reviewToPost = new()
        {
            LevelRating = RatingType.Yay,
            Labels = labels,
            Content = "This is a unique review",
        };

        ApiResponse<ApiGameReviewResponse>? response = client.PostData<ApiGameReviewResponse>($"/api/v3/levels/id/{id}/reviews", reviewToPost);

        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Success, Is.True);
        Assert.That(response.Data!.Publisher.UserId, Is.EqualTo(reviewer.UserId.ToString()));
        Assert.That(response.Data!.Level.LevelId, Is.EqualTo(id));

        Assert.That(response.Data!.Text, Is.EqualTo(reviewToPost.Content));
        Assert.That(response.Data!.LevelRating, Is.EqualTo((int)RatingType.Yay));
        Assert.That(response.Data!.LabelList, Is.EqualTo(labels.Take(UgcLimits.MaximumLabels).ToList()));
    }

    [Test]
    public void RejectReviewForUnplayedLevel()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);
        int id = level.LevelId;

        GameUser reviewer = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, reviewer);

        ApiSubmitReviewRequest reviewToPost = new()
        {
            LevelRating = RatingType.Boo,
            Content = "This level is poo and shark survival is better",
        };
        ApiResponse<ApiGameReviewResponse>? response = client.PostData<ApiGameReviewResponse>($"/api/v3/levels/id/{id}/reviews", reviewToPost, false);

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Error, Is.Not.Null);
        Assert.That(response.Error!.Name, Is.EqualTo("ApiValidationError"));
    }

    [Test]
    public void RejectReviewForNonexistentLevel()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();

        GameUser reviewer = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, reviewer);

        ApiSubmitReviewRequest reviewToPost = new()
        {
            LevelRating = RatingType.Boo,
            Content = "This stinks level",
        };
        ApiResponse<ApiGameReviewResponse>? response = client.PostData<ApiGameReviewResponse>($"/api/v3/levels/id/54321/reviews", reviewToPost, false);

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Error, Is.Not.Null);
        Assert.That(response.Error!.Name, Is.EqualTo("ApiNotFoundError"));
    }

    [Test]
    public async Task RateReview()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);
        int id = level.LevelId;

        GameUser reviewer = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, publisher);

        GameReview review = context.Database.AddReviewToLevel(new ApiSubmitReviewRequest
        {
            Content = "stinks This level"
        }, level, reviewer);

        // Now apply various ratings
        int[] ratePattern = [1, -1, 0, 1, 0, 0, -1];

        foreach (int rating in ratePattern)
        {
            Console.WriteLine(($"Coming up rating {rating}"));
            HttpResponseMessage response = await client.PostAsync($"/api/v3/reviews/id/{review.ReviewId}/rate/{rating}", null);
            Assert.That(response.IsSuccessStatusCode, Is.True);
            context.Database.Refresh();
            Assert.That(context.Database.GetRateReviewRelationForReview(publisher, review), rating == 0 ? Is.Null : Is.Not.Null);

            // Check the review's API response too
            ApiResponse<ApiGameReviewResponse>? reviewResponse = client.GetData<ApiGameReviewResponse>($"/api/v3/reviews/id/{review.ReviewId}");
            Assert.That(reviewResponse?.Data, Is.Not.Null);
            Assert.That(reviewResponse!.Success, Is.True);
            Assert.That(reviewResponse.Data!.ReviewRating.OwnRating, Is.EqualTo(rating));
        }
    }

    [Test]
    public async Task RejectInvalidReviewRating()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);
        int id = level.LevelId;

        GameUser reviewer = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, reviewer);

        GameReview review = context.Database.AddReviewToLevel(new ApiSubmitReviewRequest
        {
            Content = "stinks level This"
        }, level, reviewer);

        // Now try to apply various ratings
        string[] ratePattern = ["2", "-2", "0.125", "bad", "true"];

        foreach (string rating in ratePattern)
        {
            HttpResponseMessage response = await client.PostAsync($"/api/v3/reviews/id/{review.ReviewId}/rate/{rating}", null);
            Assert.That(response.IsSuccessStatusCode, Is.False);
            context.Database.Refresh();
            Assert.That(context.Database.GetRateReviewRelationForReview(publisher, review), Is.Null);

            // Check rating
            DatabaseRating globalRating = context.Database.GetRatingForReview(review);
            Assert.That(globalRating.PositiveRating, Is.Zero);
            Assert.That(globalRating.NegativeRating, Is.Zero);
        }
    }

    [Test]
    public async Task CantRateOwnReview()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);
        int id = level.LevelId;

        GameUser reviewer = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, reviewer);

        GameReview review = context.Database.AddReviewToLevel(new ApiSubmitReviewRequest
        {
            Content = "level stinks This"
        }, level, reviewer);

        HttpResponseMessage response = await client.PostAsync($"/api/v3/reviews/id/{review.ReviewId}/rate/1", null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(response, Is.Not.Null);
        Assert.That(context.Database.GetRateReviewRelationForReview(publisher, review), Is.Null);

        // Check rating
        DatabaseRating globalRating = context.Database.GetRatingForReview(review);
        Assert.That(globalRating.PositiveRating, Is.Zero);
        Assert.That(globalRating.NegativeRating, Is.Zero);
    }

    [Test]
    public async Task DeleteReviewAsLevelPublisher()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);
        int id = level.LevelId;

        GameUser reviewer = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, publisher);

        GameReview review = context.Database.AddReviewToLevel(new ApiSubmitReviewRequest
        {
            Content = "this level is trash, but i wont say why"
        }, level, reviewer);

        // Try to delete
        HttpResponseMessage response = await client.DeleteAsync($"/api/v3/reviews/id/{review.ReviewId}");
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(context.Database.GetReviewById(review.ReviewId), Is.Null);
    }

    [Test]
    public async Task DeleteReviewAsReviewPublisher()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);
        int id = level.LevelId;

        GameUser reviewer = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, reviewer);

        GameReview review = context.Database.AddReviewToLevel(new ApiSubmitReviewRequest
        {
            Labels = [Label.AttractTweaker]
        }, level, reviewer);

        // Try to delete
        HttpResponseMessage response = await client.DeleteAsync($"/api/v3/reviews/id/{review.ReviewId}");
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(context.Database.GetReviewById(review.ReviewId), Is.Null);
    }

    [Test]
    public async Task CantDeleteReviewIfNotPermitted()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);
        int id = level.LevelId;

        GameUser reviewer = context.CreateUser();
        GameUser moron = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, moron);

        GameReview review = context.Database.AddReviewToLevel(new ApiSubmitReviewRequest
        {
            Content = "this level is decent, but i didn't like the following: ..."
        }, level, reviewer);

        // Try to delete
        HttpResponseMessage response = await client.DeleteAsync($"/api/v3/reviews/id/{review.ReviewId}");
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(context.Database.GetReviewById(review.ReviewId), Is.Not.Null);
    }

    [Test]
    public void GetReviewsByUser()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);
        int id = level.LevelId;

        GameUser reviewer = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, reviewer);

        GameReview originalReview = context.Database.AddReviewToLevel(new ApiSubmitReviewRequest
        {
            Content = "Cooler",
            Labels = [Label.Water],
            LevelRating = RatingType.Yay
        }, level, reviewer);

        // Get by user UUID
        ApiListResponse<ApiGameReviewResponse>? reviews = context.Http.GetList<ApiGameReviewResponse>($"/api/v3/users/uuid/{reviewer.UserId}/reviews?count=240");
        Assert.That(reviews, Is.Not.Null);
        Assert.That(reviews!.Data, Has.Count.EqualTo(1));
        Assert.That(reviews.Data![0].ReviewId, Is.EqualTo(originalReview.ReviewId));

        // Get by username
        reviews = context.Http.GetList<ApiGameReviewResponse>($"/api/v3/users/username/{reviewer.Username}/reviews?count=420");
        Assert.That(reviews, Is.Not.Null);
        Assert.That(reviews!.Data, Has.Count.EqualTo(1));
        Assert.That(reviews.Data![0].ReviewId, Is.EqualTo(originalReview.ReviewId));
    }
}