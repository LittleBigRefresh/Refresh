using Refresh.Common.Constants;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request.Authentication;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Categories;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.ApiV3;

public class UserApiTests : GameServerTest
{
    [Test]
    public void GetsUserByUsername()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        
        ApiResponse<ApiGameUserResponse>? response = context.Http.GetData<ApiGameUserResponse>($"/api/v3/users/name/{user.Username}");
        Assert.That(response, Is.Not.Null);
        
        response = context.Http.GetData<ApiGameUserResponse>($"/api/v3/users/uuid/{user.Username}", false, true);
        Assert.That(response, Is.Not.Null);
        response!.AssertErrorIsEqual(ApiNotFoundError.UserMissingError);
    }

    [Test]
    public void RegisterAccount()
    {
        using TestContext context = this.GetServer();

        const string username = "a_lil_guy";
        
        ApiResponse<ApiAuthenticationResponse>? response = context.Http.PostData<ApiAuthenticationResponse>("/api/v3/register", new ApiRegisterRequest
        {
            Username = username,
            EmailAddress = "guy@lil.com",
            PasswordSha512 = "ee26b0dd4af7e749aa1a8ee3c10ae9923f618980772e473f8819a5d4940e0db27ac185f8a0e1d5f84f88bc887fd67b143732c304cc5fa9ad8e6f57f50028a8ff",
        });
        Assert.That(response, Is.Not.EqualTo(null));
        
        context.Database.Refresh();
        Assert.That(context.Database.GetUserByUsername(username), Is.Not.EqualTo(null));
    }
    
    [Test]
    public void CannotRegisterAccountWithDisallowedUsername()
    {
        using TestContext context = this.GetServer();

        const string username = "a_lil_guy";

        context.Database.DisallowUser(username);
        
        ApiResponse<ApiAuthenticationResponse>? response = context.Http.PostData<ApiAuthenticationResponse>("/api/v3/register", new ApiRegisterRequest
        {
            Username = username,
            EmailAddress = "guy@lil.com",
            PasswordSha512 = "ee26b0dd4af7e749aa1a8ee3c10ae9923f618980772e473f8819a5d4940e0db27ac185f8a0e1d5f84f88bc887fd67b143732c304cc5fa9ad8e6f57f50028a8ff",
        }, false, true);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Error, Is.Not.EqualTo(null));
        Assert.That(response.Error!.Name, Is.EqualTo("ApiAuthenticationError"));
        
        context.Database.Refresh();
        Assert.That(context.Database.GetUserByUsername(username), Is.EqualTo(null));
    }
    
    [TestCase("4")]
    [TestCase("44444444444444444444444444444444444444")]
    [TestCase("$#*(72($&8#$")]
    public void CannotRegisterAccountWithInvalidUsername(string username)
    {
        using TestContext context = this.GetServer();
        
        ApiResponse<ApiAuthenticationResponse>? response = context.Http.PostData<ApiAuthenticationResponse>("/api/v3/register", new ApiRegisterRequest
        {
            Username = username,
            EmailAddress = "far4@toolkit.vita",
            PasswordSha512 = "ee26b0dd4af7e749aa1a8ee3c10ae9923f618980772e473f8819a5d4940e0db27ac185f8a0e1d5f84f88bc887fd67b143732c304cc5fa9ad8e6f57f50028a8ff",
        }, false, true);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Error, Is.Not.Null);
        Assert.That(response.Error!.Name, Is.EqualTo("ApiValidationError"));
        
        context.Database.Refresh();
        Assert.That(context.Database.GetUserByUsername(username), Is.EqualTo(null));
    }
    
    [Test]
    public void GetsUserByUuid()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        
        ApiResponse<ApiGameUserResponse>? response = context.Http.GetData<ApiGameUserResponse>($"/api/v3/users/uuid/{user.UserId.ToString()}");
        Assert.That(response, Is.Not.Null);
        
        response = context.Http.GetData<ApiGameUserResponse>($"/api/v3/users/name/{user.UserId.ToString()}", false, true);
        Assert.That(response, Is.Not.Null);
        response!.AssertErrorIsEqual(ApiNotFoundError.UserMissingError);
    }
    
    [Test]
    public void UserNotFound()
    {
        using TestContext context = this.GetServer();
        context.CreateUser();

        ApiResponse<ApiGameUserResponse>? response = context.Http.GetData<ApiGameUserResponse>("/api/v3/users/name/dingus", false, true);
        Assert.That(response, Is.Not.Null);
        response!.AssertErrorIsEqual(ApiNotFoundError.UserMissingError);
        
        response = context.Http.GetData<ApiGameUserResponse>("/api/v3/users/uuid/dingus", false, true);
        Assert.That(response, Is.Not.Null);
        response!.AssertErrorIsEqual(ApiNotFoundError.UserMissingError);
    }
    
    [Test]
    public void DoesntGetOwnUserWhenUnauthed()
    {
        using TestContext context = this.GetServer();
        context.CreateUser();

        HttpResponseMessage response = context.Http.GetAsync("/api/v3/users/me").Result;
        Assert.That(response.StatusCode, Is.EqualTo(Forbidden));
    }
    
    [Test]
    public void GetsOwnUserWhenAuthed()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);

        ApiResponse<ApiGameUserResponse>? response = client.GetData<ApiGameUserResponse>("/api/v3/users/me");
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Data!.Username, Is.EqualTo(user.Username));
    }

    [Test]
    public void CanPatchOwnUser()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        const string description = "yeah";

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);
        
        Assert.That(user.Description, Is.EqualTo(string.Empty));

        object payload = new { description };
        ApiResponse<ApiGameUserResponse>? response = client.PatchData<ApiGameUserResponse>("/api/v3/users/me", payload);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Data!.Description, Is.EqualTo(description));

        context.Database.Refresh();
        user = context.Database.GetUserByObjectId(user.UserId)!;
        Assert.That(user.Description, Is.EqualTo(description));
    }

    [Test]
    [TestCase("")]
    [TestCase("0")]
    public void CanResetOwnIcon(string newIcon)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);
        
        // Prepare by setting icon to something
        string fakeIcon = "mmmmm";
        context.Database.UpdateUserData(user, new ApiUpdateUserRequest()
        {
            IconHash = fakeIcon
        });
        GameUser? userPrepared = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(userPrepared, Is.Not.Null);
        Assert.That(userPrepared!.IconHash, Is.EqualTo(fakeIcon));

        // Now try resetting
        ApiUpdateUserRequest request = new()
        {
            IconHash = newIcon
        };
        ApiResponse<ApiGameUserResponse>? response = client.PatchData<ApiGameUserResponse>("/api/v3/users/me", request);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Data!.IconHash, Is.EqualTo("0"));

        context.Database.Refresh();

        GameUser? userUpdated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(userUpdated, Is.Not.Null);
        Assert.That(userUpdated!.IconHash, Is.EqualTo("0"));
    }

    [Test]
    public void UpdateShowModdedPlanets()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);

        // Disable and ensure the option is set to hide
        ApiUpdateUserRequest payload = new()
        {
            ShowModdedPlanets = false,
        };
        ApiResponse<ApiExtendedGameUserResponse>? response = client.PatchData<ApiExtendedGameUserResponse>("/api/v3/users/me", payload);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Data!.ShowModdedPlanets, Is.False);

        context.Database.Refresh();
        Assert.That(context.Database.GetUserByObjectId(user.UserId)!.ShowModdedPlanets, Is.False);

        // Enable again and ensure the option is set to show
        payload.ShowModdedPlanets = true;
        response = client.PatchData<ApiExtendedGameUserResponse>("/api/v3/users/me", payload);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Data!.ShowModdedPlanets, Is.True);

        context.Database.Refresh();
        Assert.That(context.Database.GetUserByObjectId(user.UserId)!.ShowModdedPlanets, Is.True);
    }
    
    [Test]
    public void UserDescriptionGetsTrimmed()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);

        ApiUpdateUserRequest payload = new()
        {
            Description = new string('S', 600),
        };
        ApiResponse<ApiGameUserResponse>? response = client.PatchData<ApiGameUserResponse>("/api/v3/users/me", payload);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Data!.Description.Length, Is.EqualTo(UgcLimits.DescriptionLimit));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void GetsUserCategories(bool showOnlineUsers)
    {
        using TestContext context = this.GetServer();

        // Prepare config
        context.Server.Value.GameServerConfig.PermitShowingOnlineUsers = showOnlineUsers;

        ApiListResponse<ApiUserCategoryResponse>? categories = context.Http.GetList<ApiUserCategoryResponse>("/api/v3/users");
        Assert.That(categories, Is.Not.Null);

        if (!showOnlineUsers)
        {
            Assert.That(categories!.ListInfo!.TotalItems, Is.Zero);
            return;
        }

        Assert.That(categories!.ListInfo!.TotalItems, Is.EqualTo(categories.Data!.Count));
        Assert.That(categories.ListInfo.TotalItems, Is.Not.Zero);
    }
    
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void GetsUserCategoriesWithPreviews(bool showOnlineUsers)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        // Prepare config
        context.Server.Value.GameServerConfig.PermitShowingOnlineUsers = showOnlineUsers;

        ApiListResponse<ApiUserCategoryResponse>? categories = context.Http.GetList<ApiUserCategoryResponse>("/api/v3/users?includePreviews=true");
        Assert.That(categories, Is.Not.Null);

        if (!showOnlineUsers)
        {
            Assert.That(categories!.ListInfo!.TotalItems, Is.Zero);
            return;
        }
        
        ApiUserCategoryResponse? category = categories?.Data?.FirstOrDefault(c => c.ApiRoute == "newest");
        Assert.That(category, Is.Not.Null);
        
        Assert.Multiple(() =>
        {
            Assert.That(category!.PreviewItem, Is.Not.Null);
            Assert.That(category.PreviewItem!.UserId, Is.EqualTo(user.UserId.ToString()));
        });
    }
    
    [Test]
    public void DoesntGetUserCategoriesWithGarbledPreviews()
    {
        using TestContext context = this.GetServer();
        
        ApiListResponse<ApiUserCategoryResponse>? categories = context.Http.GetList<ApiUserCategoryResponse>("/api/v3/users?includePreviews=IIIIIIIIHEHAHAHAHAHAHAHA", false, true); // https://youtu.be/mpAnsf12JkA?t=2
        Assert.That(categories, Is.Not.Null);
        categories!.AssertErrorIsEqual(ApiValidationError.BooleanParseError);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void GetsNewestUsers(bool showOnlineUsers)
    {
        using TestContext context = this.GetServer();
        List<GameUser> users = [];
        const int usersCount = 10;

        // Prepare users
        for (int i = 0; i < usersCount; i++)
        {
            GameUser user = context.CreateUser();
            users.Add(user);
        }
        
        // Prepare config
        context.Server.Value.GameServerConfig.PermitShowingOnlineUsers = showOnlineUsers;

        if (!showOnlineUsers)
        {
            HttpResponseMessage message = context.Http.GetAsync("/api/v3/users/newest").Result;
            Assert.That(message.StatusCode, Is.EqualTo(NotFound));
            return;
        }

        ApiListResponse<ApiGameUserResponse>? response = context.Http.GetList<ApiGameUserResponse>("/api/v3/users/newest?count=20", false);
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response?.ListInfo, Is.Not.Null);
        Assert.That(response!.ListInfo!.TotalItems, Is.EqualTo(usersCount));
        Assert.That(response!.Data!, Has.Count.EqualTo(usersCount));

        int index = 0;
        users = users.OrderByDescending(u => u.JoinDate).ToList();
        foreach(ApiGameUserResponse user in response.Data!)
        {
            Assert.That(user.UserId, Is.EqualTo(users[index].UserId.ToString()));
            index++;
        }
    }
}