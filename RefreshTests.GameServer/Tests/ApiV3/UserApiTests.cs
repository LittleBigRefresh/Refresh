using Refresh.GameServer.Authentication;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes.Errors;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
using Refresh.GameServer.Types.UserData;

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
}