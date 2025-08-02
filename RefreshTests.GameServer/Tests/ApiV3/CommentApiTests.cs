using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Comments;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.ApiV3;

public class CommentApiTests : GameServerTest
{
    [Test]
    public void PostLevelComments()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);
        int id = level.LevelId;
        const int commentsToPostCount = 4;

        for (int i = 0; i < commentsToPostCount; i++)
        {
            GameUser newCommentPoster = context.CreateUser();
            using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, newCommentPoster);

            ApiCommentPostRequest commentToPost = new()
            {
                Content = "This level stinks"
            };
            ApiResponse<ApiLevelCommentResponse>? response = client.PostData<ApiLevelCommentResponse>($"/api/v3/levels/id/{id}/comments", commentToPost);

            // Check the returned comment's attributes
            Assert.That(response?.Data, Is.Not.Null);
            Assert.That(response!.Success, Is.True);
            Assert.That(response.Data!.Poster.UserId, Is.EqualTo(newCommentPoster.UserId.ToString()));
            Assert.That(response.Data!.Level.LevelId, Is.EqualTo(id));
            Assert.That(response.Data!.Content, Is.EqualTo(commentToPost.Content));
        }

        // Now get the comments
        ApiListResponse<ApiLevelCommentResponse>? comments = context.Http.GetList<ApiLevelCommentResponse>($"/api/v3/levels/id/{id}/comments");
        Assert.That(comments?.Data, Is.Not.Null);
        Assert.That(comments!.Success, Is.True);
        Assert.That(comments.Data!, Has.Count.EqualTo(commentsToPostCount));
    }

    [Test]
    public void PostProfileComments()
    {
        using TestContext context = this.GetServer();
        GameUser profile = context.CreateUser();
        string uuid = profile.UserId.ToString();
        const int commentsToPostCount = 4;

        for (int i = 0; i < commentsToPostCount; i++)
        {
            GameUser newCommentPoster = context.CreateUser();
            using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, newCommentPoster);

            ApiCommentPostRequest commentToPost = new()
            {
                Content = "Hi lol"
            };
            ApiResponse<ApiProfileCommentResponse>? response = client.PostData<ApiProfileCommentResponse>($"/api/v3/users/uuid/{uuid}/comments", commentToPost);

            // Check the returned comment's attributes
            Assert.That(response?.Data, Is.Not.Null);
            Assert.That(response!.Success, Is.True);
            Assert.That(response.Data!.Poster.UserId, Is.EqualTo(newCommentPoster.UserId.ToString()));
            Assert.That(response.Data!.Profile.UserId, Is.EqualTo(uuid));
            Assert.That(response.Data!.Content, Is.EqualTo(commentToPost.Content));
        }

        // Now get the comments
        ApiListResponse<ApiProfileCommentResponse>? comments = context.Http.GetList<ApiProfileCommentResponse>($"/api/v3/users/uuid/{uuid}/comments");
        Assert.That(comments?.Data, Is.Not.Null);
        Assert.That(comments!.Success, Is.True);
        Assert.That(comments.Data!, Has.Count.EqualTo(commentsToPostCount));
    }

    [Test]
    public async Task DeleteLevelCommentAsPoster()
    {
        using TestContext context = this.GetServer();
        GameUser poster = context.CreateUser();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);

        // Create and delete comment using its ID
        GameLevelComment comment = context.Database.PostCommentToLevel(level, poster, "Would be funny if i spoiled this level's ending");
        int id = comment.SequentialId;

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, poster);
        HttpResponseMessage response = await client.DeleteAsync($"/api/v3/levelComments/id/{id}");
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(context.Database.GetLevelCommentById(id), Is.Null);
    }

    [Test]
    public async Task DeleteLevelCommentAsLevelPublisher()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);

        // Create and delete comment using its ID
        GameLevelComment comment = context.Database.PostCommentToLevel(level, publisher, "h4h");
        int id = comment.SequentialId;

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, publisher);
        HttpResponseMessage response = await client.DeleteAsync($"/api/v3/levelComments/id/{id}");
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(context.Database.GetLevelCommentById(id), Is.Null);
    }

    [Test]
    public async Task CantDeleteLevelCommentIfNotPermitted()
    {
        using TestContext context = this.GetServer();
        GameUser moron = context.CreateUser();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);

        // Create and try to delete comment using its ID
        GameLevelComment comment = context.Database.PostCommentToLevel(level, publisher, "This comment is untouchable");
        int id = comment.SequentialId;

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, moron);
        HttpResponseMessage response = await client.DeleteAsync($"/api/v3/levelComments/id/{id}");
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(context.Database.GetLevelCommentById(id), Is.Not.Null);
    }

    [Test]
    public async Task DeleteProfileCommentAsPoster()
    {
        using TestContext context = this.GetServer();
        GameUser poster = context.CreateUser();
        GameUser profile = context.CreateUser();

        // Create and try to delete comment using its ID
        GameProfileComment comment = context.Database.PostCommentToProfile(profile, poster, "i ran out of funny things to put here");
        int id = comment.SequentialId;

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, poster);
        HttpResponseMessage response = await client.DeleteAsync($"/api/v3/profileComments/id/{id}");
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(context.Database.GetProfileCommentById(id), Is.Null);
    }

    [Test]
    public async Task DeleteProfileCommentAsProfileOwner()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();

        // Create and try to delete comment using its ID
        GameProfileComment comment = context.Database.PostCommentToProfile(publisher, publisher, "By visiting this profile you agree to...");
        int id = comment.SequentialId;

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, publisher);
        HttpResponseMessage response = await client.DeleteAsync($"/api/v3/profileComments/id/{id}");
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(context.Database.GetProfileCommentById(id), Is.Null);
    }

    [Test]
    public async Task CantDeleteProfileCommentIfNotPermitted()
    {
        using TestContext context = this.GetServer();
        GameUser moron = context.CreateUser();
        GameUser profile = context.CreateUser();

        // Create and try to delete comment using its ID
        GameProfileComment comment = context.Database.PostCommentToProfile(profile, profile, "im commenting on my own profile im so original");
        int id = comment.SequentialId;

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, moron);
        HttpResponseMessage response = await client.DeleteAsync($"/api/v3/profileComments/id/{id}");
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(context.Database.GetProfileCommentById(id), Is.Not.Null);
    }

    [Test]
    public async Task RateLevelComment()
    {
        using TestContext context = this.GetServer();
        GameUser rater = context.CreateUser();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);
        int[] ratePattern = [1, -1, 0, 1, 0, 0, -1];

        GameLevelComment comment = context.Database.PostCommentToLevel(level, publisher, "We've tested all obstacles and can confirm this level is no-hittable");
        int id = comment.SequentialId;

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, rater);

        foreach (int rating in ratePattern)
        {
            HttpResponseMessage response = await client.PostAsync($"/api/v3/levelComments/id/{id}/rate/{rating}", null);
            Assert.That(response.IsSuccessStatusCode, Is.True);
            Assert.That(context.Database.GetLevelCommentRatingByUser(comment, rater), rating == 0 ? Is.Null : Is.Not.Null);
        }
    }

    [Test]
    [TestCase("-2")]
    [TestCase("2")]
    [TestCase("0.125")]
    [TestCase("1234567")]
    [TestCase("-1234567")]
    [TestCase("12345678900987654321")]
    [TestCase("")]
    [TestCase("agree")]
    public async Task CantRateLevelCommentWithInvalidRating(string rawRating)
    {
        using TestContext context = this.GetServer();
        GameUser rater = context.CreateUser();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);

        GameLevelComment comment = context.Database.PostCommentToLevel(level, publisher, "play this");
        int id = comment.SequentialId;

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, rater);
        HttpResponseMessage response = await client.PostAsync($"/api/v3/levelComments/id/{id}/rate/{rawRating}", null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(context.Database.GetLevelCommentRatingByUser(comment, rater), Is.Null);
    }

    [Test]
    public async Task RateProfileComment()
    {
        using TestContext context = this.GetServer();
        GameUser rater = context.CreateUser();
        GameUser profile = context.CreateUser();
        int[] ratePattern = [1, -1, 0, 1, 0, 0, -1];

        GameProfileComment comment = context.Database.PostCommentToProfile(profile, profile, "test text");
        int id = comment.SequentialId;

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, rater);

        foreach (int rating in ratePattern)
        {
            HttpResponseMessage response = await client.PostAsync($"/api/v3/profileComments/id/{id}/rate/{rating}", null);
            Assert.That(response.IsSuccessStatusCode, Is.True);
            Assert.That(context.Database.GetProfileCommentRatingByUser(comment, rater), rating == 0 ? Is.Null : Is.Not.Null);
        }
    }

    [Test]
    [TestCase("-2")]
    [TestCase("2")]
    [TestCase("-0.125")]
    [TestCase("1234567")]
    [TestCase("-1234567")]
    [TestCase("1234567890098765420")]
    [TestCase("")]
    [TestCase("disagree")]
    public async Task CantRateProfileCommentWithInvalidRating(string rawRating)
    {
        using TestContext context = this.GetServer();
        GameUser rater = context.CreateUser();
        GameUser profile = context.CreateUser();

        GameProfileComment comment = context.Database.PostCommentToProfile(profile, profile, "play my levels");
        int id = comment.SequentialId;

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, rater);
        HttpResponseMessage response = await client.PostAsync($"/api/v3/profileComments/id/{id}/rate/{rawRating}", null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(context.Database.GetProfileCommentRatingByUser(comment, rater), Is.Null);
    }
}