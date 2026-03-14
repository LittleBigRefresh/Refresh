using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request.Authentication;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.ApiV3;

public class UserDeletionApiTests : GameServerTest
{
    [Test]
    public void CanDeleteOwnUserWithPassword()
    {
        using TestContext context = this.GetServer();
        const string password = "password";

        GameUser user = context.CreateUser();
        string passwordBcrypt = BCrypt.Net.BCrypt.HashPassword(password, 4);
        context.Database.SetUserPassword(user, passwordBcrypt);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);

        Assert.That(context.Database.GetUserByObjectId(user.UserId), Is.Not.Null);

        // Try to delete
        ApiOwnUserDeletionRequest request = new()
        {
            PasswordSha512 = password
        };
        ApiResponse<ApiEmptyResponse>? response = client.DeleteData<ApiEmptyResponse>($"/api/v3/users/me", request);
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Success, Is.True);

        context.Database.Refresh();
        GameUser? fakeDeletedUser = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(fakeDeletedUser, Is.Not.Null);
        Assert.That(fakeDeletedUser!.Role, Is.EqualTo(GameUserRole.Banned));
        Assert.That(fakeDeletedUser!.PasswordBcrypt, Is.EqualTo("deleted"));
    }

    [Test]
    public async Task CannotDeleteOwnUserWithoutPassword()
    {
        using TestContext context = this.GetServer();
        const string password = "password";

        GameUser user = context.CreateUser();
        string passwordBcrypt = BCrypt.Net.BCrypt.HashPassword(password, 4);
        context.Database.SetUserPassword(user, passwordBcrypt);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);

        Assert.That(context.Database.GetUserByObjectId(user.UserId), Is.Not.Null);

        // Try to delete
        HttpResponseMessage? response = await client.DeleteAsync($"/api/v3/users/me");
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.IsSuccessStatusCode, Is.False);

        context.Database.Refresh();
        GameUser? fakeDeletedUser = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(fakeDeletedUser, Is.Not.Null);
        Assert.That(fakeDeletedUser!.Role, Is.EqualTo(GameUserRole.User));
        Assert.That(fakeDeletedUser!.PasswordBcrypt, Is.Not.EqualTo("deleted"));
    }

    [Test]
    public void CannotDeleteOwnUserWithWrongPassword()
    {
        using TestContext context = this.GetServer();
        const string correctPassword = "password";
        const string wrongPassword = "assword";

        GameUser user = context.CreateUser();
        string correctPasswordBcrypt = BCrypt.Net.BCrypt.HashPassword(correctPassword, 4);
        string wrongPasswordBcrypt = BCrypt.Net.BCrypt.HashPassword(wrongPassword, 4);
        context.Database.SetUserPassword(user, correctPasswordBcrypt);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);

        Assert.That(context.Database.GetUserByObjectId(user.UserId), Is.Not.Null);

        // Try to delete
        ApiOwnUserDeletionRequest request = new()
        {
            PasswordSha512 = wrongPassword
        };
        ApiResponse<ApiEmptyResponse>? response = client.DeleteData<ApiEmptyResponse>($"/api/v3/users/me", request, false, true);
        Assert.That(response?.Error, Is.Not.Null);
        Assert.That(response!.Success, Is.False);

        context.Database.Refresh();
        GameUser? fakeDeletedUser = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(fakeDeletedUser, Is.Not.Null);
        Assert.That(fakeDeletedUser!.Role, Is.EqualTo(GameUserRole.User));
        Assert.That(fakeDeletedUser!.PasswordBcrypt, Is.Not.EqualTo("deleted"));
    }
} 