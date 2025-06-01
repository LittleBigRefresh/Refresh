using Refresh.Database.Models.Authentication;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Request.Authentication;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response.Users;
using Refresh.Database.Models.Users;
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
        
        response = context.Http.GetData<ApiGameUserResponse>($"/api/v3/users/uuid/{user.Username}");
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
        });
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
        });
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
        
        response = context.Http.GetData<ApiGameUserResponse>($"/api/v3/users/name/{user.UserId.ToString()}");
        Assert.That(response, Is.Not.Null);
        response!.AssertErrorIsEqual(ApiNotFoundError.UserMissingError);
    }
    
    [Test]
    public void UserNotFound()
    {
        using TestContext context = this.GetServer();
        context.CreateUser();

        ApiResponse<ApiGameUserResponse>? response = context.Http.GetData<ApiGameUserResponse>("/api/v3/users/name/dingus");
        Assert.That(response, Is.Not.Null);
        response!.AssertErrorIsEqual(ApiNotFoundError.UserMissingError);
        
        response = context.Http.GetData<ApiGameUserResponse>("/api/v3/users/uuid/dingus");
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
        
        Assert.That(user.Description, Is.EqualTo(description));
    }
}