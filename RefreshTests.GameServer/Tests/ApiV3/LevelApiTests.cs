using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using RefreshTests.GameServer.Extensions;
using Refresh.Database.Models.Levels;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Levels;

namespace RefreshTests.GameServer.Tests.ApiV3;

public class LevelApiTests : GameServerTest
{
    [Test]
    public void SlotsHeartedFailsWhenNoUser()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);
        
        HttpResponseMessage message = context.Http.GetAsync("/api/v3/levels/hearted").Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }

    [Test]
    public void GetsLevelCategories()
    {
        using TestContext context = this.GetServer();

        ApiListResponse<ApiLevelCategoryResponse>? categories = context.Http.GetList<ApiLevelCategoryResponse>("/api/v3/levels");
        Assert.That(categories, Is.Not.Null);
        Assert.That(categories!.ListInfo!.TotalItems, Is.EqualTo(categories.Data!.Count));
        Assert.That(categories.ListInfo.TotalItems, Is.Not.Zero);
    }
    
    [Test]
    public void GetsLevelCategoriesWithPreviews()
    {
        using TestContext context = this.GetServer();
        GameLevel level = context.CreateLevel(context.CreateUser());

        ApiListResponse<ApiLevelCategoryResponse>? categories = context.Http.GetList<ApiLevelCategoryResponse>("/api/v3/levels?includePreviews=true");
        Assert.That(categories, Is.Not.Null);
        
        ApiLevelCategoryResponse? category = categories?.Data?.FirstOrDefault(c => c.ApiRoute == "newest");
        Assert.That(category, Is.Not.Null);
        
        Assert.Multiple(() =>
        {
            Assert.That(category!.PreviewLevel, Is.Not.Null);
            Assert.That(category.PreviewLevel!.LevelId, Is.EqualTo(level.LevelId));
        });
    }
    
    [Test]
    public void DoesntGetLevelCategoriesWithGarbledPreviews()
    {
        using TestContext context = this.GetServer();
        
        ApiListResponse<ApiLevelCategoryResponse>? categories = context.Http.GetList<ApiLevelCategoryResponse>("/api/v3/levels?includePreviews=babelababehbaooh"); // https://youtu.be/K4w1h_r8l2Y?t=17
        Assert.That(categories, Is.Not.Null);
        categories!.AssertErrorIsEqual(ApiValidationError.BooleanParseError);
    }

    [Test]
    public void GetsNewestLevels()
    {
        using TestContext context = this.GetServer();
        GameLevel level = context.CreateLevel(context.CreateUser());

        ApiListResponse<ApiGameLevelResponse>? levels = context.Http.GetList<ApiGameLevelResponse>("/api/v3/levels/newest?count=1");
        Assert.That(levels, Is.Not.Null);
        
        Assert.That(levels!.Data, Has.Count.EqualTo(1));
        Assert.That(levels.Data![0].LevelId, Is.EqualTo(level.LevelId));
    }
    
    [Test]
    public void GetsLevelById()
    {
        using TestContext context = this.GetServer();
        GameLevel level = context.CreateLevel(context.CreateUser());

        ApiResponse<ApiGameLevelResponse>? levelResponse = context.Http.GetData<ApiGameLevelResponse>($"/api/v3/levels/id/{level.LevelId}");
        Assert.That(levelResponse, Is.Not.Null);
        Assert.That(levelResponse!.Data, Is.Not.Null);
        Assert.That(levelResponse.Data!.LevelId, Is.EqualTo(level.LevelId));
    }
    
    [Test]
    public void DoesntGetLevelByInvalidId()
    {
        using TestContext context = this.GetServer();
        context.CreateLevel(context.CreateUser());

        ApiResponse<ApiGameLevelResponse>? levelResponse = context.Http.GetData<ApiGameLevelResponse>($"/api/v3/levels/id/{int.MaxValue}");
        Assert.That(levelResponse, Is.Not.Null);
        levelResponse!.AssertErrorIsEqual(ApiNotFoundError.LevelMissingError);
    }

    [Test]
    public async Task CanDeleteLevel()
    {
        using TestContext context = this.GetServer();
        GameUser author = context.CreateUser();
        GameLevel level = context.CreateLevel(author);

        int id = level.LevelId;

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, author);
        HttpResponseMessage response = await client.DeleteAsync($"/api/v3/levels/id/{id}");
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(context.Database.GetLevelById(id), Is.Null);
    }
    
    [Test]
    public async Task CantDeleteLevelIfNotAuthor()
    {
        using TestContext context = this.GetServer();
        GameUser author = context.CreateUser();
        GameUser moron = context.CreateUser();
        GameLevel level = context.CreateLevel(author);

        int id = level.LevelId;

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, moron);
        HttpResponseMessage response = await client.DeleteAsync($"/api/v3/levels/id/{id}");
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(context.Database.GetLevelById(id), Is.Not.Null);
    }
    
    [Test]
    public async Task CantDeleteLevelIfLevelInvalid()
    {
        using TestContext context = this.GetServer();
        GameUser author = context.CreateUser();
        GameLevel level = context.CreateLevel(author);

        int id = level.LevelId;

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, author);
        HttpResponseMessage response = await client.DeleteAsync($"/api/v3/levels/id/{int.MaxValue}");
        Assert.That(response.StatusCode, Is.EqualTo(NotFound));
        Assert.That(context.Database.GetLevelById(id), Is.Not.Null);
    }
}