using MongoDB.Bson;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Moderation;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.ApiV3;

public class ModerationApiTests : GameServerTest
{
    [Test]
    [TestCase(GameUserRole.Restricted)]
    [TestCase(GameUserRole.User)]
    [TestCase(GameUserRole.Curator)]
    [TestCase(GameUserRole.Moderator)]
    [TestCase(GameUserRole.Admin)]
    public void MayEditOtherUsersProfile(GameUserRole actorRole)
    {
        using TestContext context = this.GetServer();
        GameUser actor = context.CreateUser(null, actorRole);
        GameUser target = context.CreateUser(null, GameUserRole.User);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, actor);

        ApiAdminUpdateUserRequest request = new()
        {
            Description = "lol"
        };
        ApiResponse<ApiExtendedGameUserResponse>? response = client.PatchData<ApiExtendedGameUserResponse>($"/api/v3/admin/users/uuid/{target.UserId}", request, false, false);

        context.Database.Refresh();

        if (actorRole < GameUserRole.Moderator)
        {
            // In this case response altogether is null because RoleService is the one to return Unauthorized, and it doesn't include any response body.
            // But we only care about Data being null in order to be able to tell that the request has failed.
            Assert.That(response?.Data, Is.Null);
            
            GameUser? targetUpdated = context.Database.GetUserByObjectId(target.UserId);
            Assert.That(targetUpdated, Is.Not.Null);
            Assert.That(targetUpdated!.Description, Is.Empty);
            Assert.That(context.Database.GetModerationActionsForObject(target.UserId.ToString(), ModerationObjectType.User, 0, 1).TotalItems, Is.Zero);
        }
        else
        {
            Assert.That(response?.Data, Is.Not.Null);
            Assert.That(response!.Data!.UserId.ToString(), Is.EqualTo(target.UserId.ToString()));

            GameUser? targetUpdated = context.Database.GetUserByObjectId(target.UserId);
            Assert.That(targetUpdated, Is.Not.Null);
            Assert.That(targetUpdated!.Description, Is.EqualTo("lol"));
            Assert.That(context.Database.GetModerationActionsForObject(target.UserId.ToString(), ModerationObjectType.User, 0, 1).TotalItems, Is.EqualTo(1));
        }
    }

    [Test]
    [TestCase(GameUserRole.Restricted)]
    [TestCase(GameUserRole.User)]
    [TestCase(GameUserRole.Curator)]
    [TestCase(GameUserRole.Moderator)]
    [TestCase(GameUserRole.Admin)]
    public void MayEditOtherUsersRole(GameUserRole actorRole)
    {
        using TestContext context = this.GetServer();
        GameUser actor = context.CreateUser(null, actorRole);
        GameUser target = context.CreateUser(null, GameUserRole.User);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, actor);

        ApiAdminUpdateUserRequest request = new()
        {
            Role = GameUserRole.Trusted
        };
        ApiResponse<ApiExtendedGameUserResponse>? response = client.PatchData<ApiExtendedGameUserResponse>($"/api/v3/admin/users/uuid/{target.UserId}", request, false, false);

        context.Database.Refresh();

        if (actorRole < GameUserRole.Admin)
        {
            // Error is either Unauthorized with no body if it was blocked by RoleService, or 400 with a body if it's blocked by the method
            // (happens if the user is a moderator). Understand both cases as a failure.
            Assert.That(response?.Data, Is.Null);
            
            GameUser? targetUpdated = context.Database.GetUserByObjectId(target.UserId);
            Assert.That(targetUpdated, Is.Not.Null);
            Assert.That(targetUpdated!.Role, Is.EqualTo(GameUserRole.User));
            Assert.That(context.Database.GetModerationActionsForObject(target.UserId.ToString(), ModerationObjectType.User, 0, 1).TotalItems, Is.Zero);
        }
        else
        {
            Assert.That(response?.Data, Is.Not.Null);
            Assert.That(response!.Data!.UserId.ToString(), Is.EqualTo(target.UserId.ToString()));

            GameUser? targetUpdated = context.Database.GetUserByObjectId(target.UserId);
            Assert.That(targetUpdated, Is.Not.Null);
            Assert.That(targetUpdated!.Role, Is.EqualTo(GameUserRole.Trusted));
            Assert.That(context.Database.GetModerationActionsForObject(target.UserId.ToString(), ModerationObjectType.User, 0, 1).TotalItems, Is.EqualTo(1));
        }
    }

    [Test]
    [TestCase(GameUserRole.Restricted)]
    [TestCase(GameUserRole.User)]
    [TestCase(GameUserRole.Curator)]
    [TestCase(GameUserRole.Moderator)]
    [TestCase(GameUserRole.Admin)]
    public void MayRenameOtherUser(GameUserRole actorRole)
    {
        string initialUsername = "hiii";
        string newUsername = "lolol";

        using TestContext context = this.GetServer();
        GameUser actor = context.CreateUser(role: actorRole);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, actor);

        GameUser target = context.CreateUser(initialUsername, GameUserRole.User);

        ApiAdminUpdateUserRequest request = new()
        {
            Username = newUsername
        };
        ApiResponse<ApiExtendedGameUserResponse>? response = client.PatchData<ApiExtendedGameUserResponse>($"/api/v3/admin/users/uuid/{target.UserId}", request, false, false);

        context.Database.Refresh();

        if (actorRole < GameUserRole.Moderator)
        {
            Assert.That(response?.Data, Is.Null);
            
            GameUser? targetUpdated = context.Database.GetUserByObjectId(target.UserId);
            Assert.That(targetUpdated, Is.Not.Null);
            Assert.That(targetUpdated!.Username, Is.EqualTo(initialUsername));
            Assert.That(context.Database.GetNotificationCountByUser(target), Is.Zero);
            Assert.That(context.Database.GetModerationActionsForObject(target.UserId.ToString(), ModerationObjectType.User, 0, 1).TotalItems, Is.Zero);
        }
        else
        {
            Assert.That(response?.Data, Is.Not.Null);
            Assert.That(response!.Data!.UserId.ToString(), Is.EqualTo(target.UserId.ToString()));

            GameUser? targetUpdated = context.Database.GetUserByObjectId(target.UserId);
            Assert.That(targetUpdated, Is.Not.Null);
            Assert.That(targetUpdated!.Username, Is.EqualTo(newUsername));

            Assert.That(context.Database.GetNotificationCountByUser(target), Is.EqualTo(1));
            Assert.That(context.Database.GetModerationActionsForObject(target.UserId.ToString(), ModerationObjectType.User, 0, 1).TotalItems, Is.EqualTo(1));
        }
    }

    [Test]
    [TestCase("")]
    [TestCase("0")]
    public void IconGetsReset(string newIcon)
    {
        using TestContext context = this.GetServer();
        GameUser actor = context.CreateUser(null, GameUserRole.Moderator);
        GameUser target = context.CreateUser(null, GameUserRole.User);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, actor);

        // Prepare
        string fakeIconHash = "asdfcgvhbjnkmlö";
        context.Database.UpdateUserData(target, new ApiAdminUpdateUserRequest()
        {
            IconHash = fakeIconHash
        });

        Assert.That(context.Database.GetUserByObjectId(target.UserId)?.IconHash, Is.EqualTo(fakeIconHash));

        // Now try resetting
        ApiUpdateUserRequest request = new()
        {
            IconHash = newIcon
        };
        ApiResponse<ApiGameUserResponse>? response = client.PatchData<ApiGameUserResponse>($"/api/v3/admin/users/uuid/{target.UserId}", request);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Data!.IconHash, Is.EqualTo("0"));

        context.Database.Refresh();

        GameUser? userUpdated = context.Database.GetUserByObjectId(target.UserId);
        Assert.That(userUpdated, Is.Not.Null);
        Assert.That(userUpdated!.IconHash, Is.EqualTo("0"));
    }

    [Test]
    public void CannotEditUnknownUser()
    {
        using TestContext context = this.GetServer();
        GameUser actor = context.CreateUser(null, GameUserRole.Moderator);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, actor);

        ApiAdminUpdateUserRequest request = new()
        {
            Description = "lol"
        };
        ApiResponse<ApiExtendedGameUserResponse>? response = client.PatchData<ApiExtendedGameUserResponse>($"/api/v3/admin/users/uuid/{ObjectId.GenerateNewId()}", request, false, true);
        Assert.That(response?.Error, Is.Not.Null);
        Assert.That(response!.Error!.StatusCode, Is.EqualTo(NotFound));
    }

    [Test]
    public void CannotEditUserIfIdTypeIsUnknown()
    {
        using TestContext context = this.GetServer();
        GameUser actor = context.CreateUser(null, GameUserRole.Moderator);
        GameUser target = context.CreateUser(null, GameUserRole.User);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, actor);

        ApiAdminUpdateUserRequest request = new()
        {
            Description = "lol"
        };
        ApiResponse<ApiExtendedGameUserResponse>? response = client.PatchData<ApiExtendedGameUserResponse>($"/api/v3/admin/users/mmmmmmm/{target.UserId}", request, false, true);
        Assert.That(response?.Error, Is.Not.Null);
        Assert.That(response!.Error!.StatusCode, Is.EqualTo(NotFound));
    }

    [Test]
    public void CannotRenameToTakenUsername()
    {
        using TestContext context = this.GetServer();
        string takenUsername = "lel";
        GameUser actor = context.CreateUser(takenUsername, GameUserRole.Moderator);
        GameUser target = context.CreateUser(null, GameUserRole.User);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, actor);

        ApiAdminUpdateUserRequest request = new()
        {
            Username = takenUsername
        };
        ApiResponse<ApiExtendedGameUserResponse>? response = client.PatchData<ApiExtendedGameUserResponse>($"/api/v3/admin/users/uuid/{target.UserId}", request, false, true);
        Assert.That(response?.Error, Is.Not.Null);
        Assert.That(response!.Error!.StatusCode, Is.EqualTo(BadRequest));

        context.Database.Refresh();

        GameUser? updated = context.Database.GetUserByObjectId(target.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Username, Is.EqualTo(target.Username));
        Assert.That(updated!.Username, Is.Not.EqualTo(takenUsername));
    }

    [Test]
    [TestCase("!jeff", true)]
    [TestCase("dddd", true)]
    [TestCase("dd", false)]
    [TestCase("dddddddddddddddddd", false)]
    [TestCase("jeff?", false)]
    public void OnlyRenameToValidUsernames(string newUsername, bool isValid)
    {
        using TestContext context = this.GetServer();
        GameUser actor = context.CreateUser(null, GameUserRole.Moderator);
        GameUser target = context.CreateUser(null, GameUserRole.User);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, actor);

        ApiAdminUpdateUserRequest request = new()
        {
            Username = newUsername
        };
        ApiResponse<ApiExtendedGameUserResponse>? response = client.PatchData<ApiExtendedGameUserResponse>($"/api/v3/admin/users/uuid/{target.UserId}", request, false, false);
        
        context.Database.Refresh();

        if (isValid)
        {
            Assert.That(response?.Error, Is.Null);
            Assert.That(response?.Data, Is.Not.Null);
            Assert.That(response!.Data!.Username, Is.EqualTo(newUsername));

            GameUser? updated = context.Database.GetUserByObjectId(target.UserId);
            Assert.That(updated, Is.Not.Null);
            Assert.That(updated!.Username, Is.EqualTo(newUsername));
            Assert.That(updated!.Username, Is.Not.EqualTo(target.Username));
        }
        else
        {
            Assert.That(response?.Error, Is.Not.Null);
            Assert.That(response!.Error!.StatusCode, Is.EqualTo(BadRequest));

            GameUser? updated = context.Database.GetUserByObjectId(target.UserId);
            Assert.That(updated, Is.Not.Null);
            Assert.That(updated!.Username, Is.EqualTo(target.Username));
            Assert.That(updated!.Username, Is.Not.EqualTo(newUsername));
        }
    }
}