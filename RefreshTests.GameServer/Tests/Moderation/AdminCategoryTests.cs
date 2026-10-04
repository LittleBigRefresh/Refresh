using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Categories;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;
using Refresh.Interfaces.Game.Types.Lists;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Moderation;

public class AdminCategoryTests : GameServerTest
{
    private void PrepareUsers(TestContext context)
    {
        for (int i = 0; i < 10; i++)
        {
            context.CreateUser();
        }
        
        // Deleting an extra user sets their email address to null, so we can just easily do that for testing.
        GameUser noEmailUser = context.CreateUser("nomail");
        context.Database.DeleteUser(noEmailUser);
        
        // Ensure it's null
        GameUser? noEmailUserCheck = context.Database.GetUserByObjectId(noEmailUser.UserId);
        Assert.That(noEmailUserCheck, Is.Not.Null);
        Assert.That(noEmailUserCheck!.EmailAddress, Is.Null);
    }
    
    [Test]
    public void AdminCategoriesInaccessibleViaApiIfUnauthed()
    {
        using TestContext context = this.GetServer();
        this.PrepareUsers(context);

        // Can't discover them
        HttpResponseMessage message = context.Http.GetAsync("/api/v3/admin/userCategories").Result;
        Assert.That(message.StatusCode, Is.EqualTo(Forbidden));

        // Can't use newest user redirect
        message = context.Http.GetAsync("/api/v3/admin/users").Result;
        Assert.That(message.StatusCode, Is.EqualTo(Forbidden));

        // Can't use the 2 existing categories
        message = context.Http.GetAsync("/api/v3/admin/userCategories/newest").Result;
        Assert.That(message.StatusCode, Is.EqualTo(Forbidden));
        message = context.Http.GetAsync("/api/v3/admin/userCategories/searchAddress?query=local").Result;
        Assert.That(message.StatusCode, Is.EqualTo(Forbidden));
        
        // Can't use non-existent category
        message = context.Http.GetAsync("/api/v3/admin/userCategories/grinchy").Result;
        Assert.That(message.StatusCode, Is.EqualTo(Forbidden));
    }
    
    [Test]
    [TestCase(GameUserRole.User)]
    [TestCase(GameUserRole.Restricted)]
    public void AdminCategoriesInaccessibleToNonStaff(GameUserRole role)
    {
        using TestContext context = this.GetServer();
        this.PrepareUsers(context);
        GameUser accessor = context.CreateUser(role: role);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, accessor);

        // Can't discover them
        HttpResponseMessage message = client.GetAsync("/api/v3/admin/userCategories").Result;
        Assert.That(message.StatusCode, Is.EqualTo(Unauthorized));

        // Can't use newest user redirect
        message = client.GetAsync("/api/v3/admin/users").Result;
        Assert.That(message.StatusCode, Is.EqualTo(Unauthorized));

        // Can't use the 2 existing categories
        message = client.GetAsync("/api/v3/admin/userCategories/newest").Result;
        Assert.That(message.StatusCode, Is.EqualTo(Unauthorized));
        message = client.GetAsync("/api/v3/admin/userCategories/searchAddress?query=local").Result;
        Assert.That(message.StatusCode, Is.EqualTo(Unauthorized));
        
        // Can't use non-existent category
        message = client.GetAsync("/api/v3/admin/userCategories/grinchy").Result;
        Assert.That(message.StatusCode, Is.EqualTo(Unauthorized));
    }
    
    [Test]
    [TestCase(GameUserRole.Admin)]
    [TestCase(GameUserRole.Moderator)]
    public void AdminCategoriesAccessibleToStaff(GameUserRole role)
    {
        using TestContext context = this.GetServer();
        this.PrepareUsers(context);
        GameUser accessor = context.CreateUser(role: role);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, accessor);

        // Can discover them
        ApiListResponse<ApiCategoryResponse>? categories = client.GetList<ApiCategoryResponse>("/api/v3/admin/userCategories");
        Assert.That(categories?.Data, Is.Not.Null);
        Assert.That(categories!.Data!.Any(c => c.ApiRoute == "searchAddress"), Is.True);

        // Can use newest user redirect
        ApiListResponse<ApiExtendedGameUserResponse>? list = client.GetList<ApiExtendedGameUserResponse>("/api/v3/admin/users");
        Assert.That(list?.Data, Is.Not.Null);
        Assert.That(list!.ListInfo, Is.Not.Null);
        Assert.That(list!.ListInfo!.TotalItems, Is.EqualTo(12)); // our 10 users + the mod + deleted user
        Assert.That(list!.ListInfo!.TotalItems, Is.EqualTo(list!.Data!.Count));

        // Can use the extended newest API category
        list = client.GetList<ApiExtendedGameUserResponse>("/api/v3/admin/userCategories/newest");
        Assert.That(list?.Data, Is.Not.Null);
        Assert.That(list!.ListInfo, Is.Not.Null);
        Assert.That(list!.ListInfo!.TotalItems, Is.EqualTo(12)); // our 10 users + the mod + deleted user
        Assert.That(list!.ListInfo!.TotalItems, Is.EqualTo(list!.Data!.Count));
        Assert.That(list!.Data.Any(u => u.Username == "nomail"), Is.True);
        
        // Can search users (query correctly affects results)
        list = client.GetList<ApiExtendedGameUserResponse>("/api/v3/admin/userCategories/searchAddress?query=101");
        Assert.That(list?.Data, Is.Not.Null);
        Assert.That(list!.ListInfo, Is.Not.Null);
        Assert.That(list!.ListInfo!.TotalItems, Is.EqualTo(1)); // the one user who has 101 in their email
        Assert.That(list!.ListInfo!.TotalItems, Is.EqualTo(list!.Data!.Count));
        Assert.That(list!.Data.First().EmailAddress, Is.EqualTo("101@101.local"));
        
        list = client.GetList<ApiExtendedGameUserResponse>("/api/v3/admin/userCategories/searchAddress?query=local");
        Assert.That(list?.Data, Is.Not.Null);
        Assert.That(list!.ListInfo, Is.Not.Null);
        Assert.That(list!.ListInfo!.TotalItems, Is.EqualTo(11)); // deleted user won't show due to null address
        Assert.That(list!.ListInfo!.TotalItems, Is.EqualTo(list!.Data!.Count));
        Assert.That(list!.Data.All(u => u.EmailAddress != null && u.EmailAddress.Contains("local")), Is.True);
        Assert.That(list!.Data.Any(u => u.Username == "nomail"), Is.False);
        
        // Missing query should fail
        list = client.GetList<ApiExtendedGameUserResponse>("/api/v3/admin/userCategories/searchAddress", false, true);
        Assert.That(list?.Error, Is.Not.Null);
        Assert.That(list?.Data, Is.Null);
        
        // Can't use non-existent category
        list = client.GetList<ApiExtendedGameUserResponse>("/api/v3/admin/userCategories/grinchy", false, true);
        Assert.That(list?.Error, Is.Not.Null);
        Assert.That(list!.Error!.Name, Is.EqualTo("ApiNotFoundError"));
    }
    
    [Test]
    [TestCase(GameUserRole.Admin)]
    [TestCase(GameUserRole.Moderator)]
    [TestCase(GameUserRole.User)]
    [TestCase(GameUserRole.Restricted)]
    public void SearchByAddressInaccessibleIfLBP3Category(GameUserRole role)
    {
        using TestContext context = this.GetServer();
        this.PrepareUsers(context);
        GameUser accessor = context.CreateUser(role: role);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, accessor);
        
        // Ensure it's not discoverable
        HttpResponseMessage message = client.GetAsync($"/lbp/searches?count=67").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        // Make sure neither the address category isn't in the list nor the dedicated search property
        SerializedCategoryList result = message.Content.ReadAsXML<SerializedCategoryList>();
        Assert.That(result.Items.Any(c => c.Url.Contains("searchAddress")), Is.False);
        Assert.That(result.TextSearchCategory.Url, Does.Not.Contain("searchAddress"));
        
        // Also do this for the genre categories
        message = client.GetAsync($"/lbp/genres?count=67").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        result = message.Content.ReadAsXML<SerializedCategoryList>();
        Assert.That(result.Items.Any(c => c.Url.Contains("searchAddress")), Is.False);
        Assert.That(result.TextSearchCategory.Url, Does.Not.Contain("searchAddress"));
        
        // Ensure it can't be requested separately
        message = client.GetAsync($"/lbp/searches/users/searchAddress").Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
        message = client.GetAsync($"/lbp/searches/levels/searchAddress").Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    [TestCase(GameUserRole.Admin)]
    [TestCase(GameUserRole.Moderator)]
    [TestCase(GameUserRole.User)]
    [TestCase(GameUserRole.Restricted)]
    public void SearchByAddressInaccessibleIfLBPSlotsEndpoint(GameUserRole role)
    {
        using TestContext context = this.GetServer();
        this.PrepareUsers(context);
        GameUser accessor = context.CreateUser(role: role);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet2, TokenPlatform.PS3, accessor);
        
        // Ensure it's also not somehow accessible via /slots/{route}, whyever that would happen
        HttpResponseMessage message = client.GetAsync($"/lbp/slots/searchAddress?count=67").Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SearchByAddressWorksRegardlessOfConfig(bool permitShowingOnlineUsers)
    {
        using TestContext context = this.GetServer();
        this.PrepareUsers(context);
        GameUser accessor = context.CreateUser(role: GameUserRole.Moderator);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, accessor);
        
        context.Server.Value.GameServerConfig.PermitShowingOnlineUsers = permitShowingOnlineUsers;

        // Can receive users even if they are hidden for regular users
        ApiListResponse<ApiExtendedGameUserResponse>? list = client.GetList<ApiExtendedGameUserResponse>("/api/v3/admin/userCategories/newest");
        Assert.That(list?.Data, Is.Not.Null);
        Assert.That(list!.ListInfo, Is.Not.Null);
        Assert.That(list!.ListInfo!.TotalItems, Is.EqualTo(12));
        Assert.That(list!.ListInfo!.TotalItems, Is.EqualTo(list!.Data!.Count));
        
        list = client.GetList<ApiExtendedGameUserResponse>("/api/v3/admin/userCategories/searchAddress?query=local");
        Assert.That(list?.Data, Is.Not.Null);
        Assert.That(list!.ListInfo, Is.Not.Null);
        Assert.That(list!.ListInfo!.TotalItems, Is.EqualTo(11));
        Assert.That(list!.ListInfo!.TotalItems, Is.EqualTo(list!.Data!.Count));
    }
}