using MongoDB.Bson;
using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Moderation;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.ApiV3;

public class AdminUserEditApiTests : GameServerTest
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
    public void EditsUserByUuidAndName()
    {
        using TestContext context = this.GetServer();
        GameUser mod = context.CreateUser(role: GameUserRole.Moderator);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, mod);
        GameUser player = context.CreateUser(role: GameUserRole.User);

        // UUID
        ApiAdminUpdateUserRequest request = new()
        {
            Description = "poo"
        };
        ApiResponse<ApiExtendedGameUserResponse>? response = client.PatchData<ApiExtendedGameUserResponse>($"/api/v3/admin/users/uuid/{player.UserId}", request);
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.Description, Is.EqualTo(request.Description));

        // name
        request = new()
        {
            Description = "lmao"
        };
        response = client.PatchData<ApiExtendedGameUserResponse>($"/api/v3/admin/users/name/{player.Username}", request);
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.Description, Is.EqualTo(request.Description));
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
    public void CanRenameUserToDifferentUsersPreviousName()
    {
        using TestContext context = this.GetServer();

        GameUser mod = context.CreateUser(null, GameUserRole.Moderator);
        GameUser owner = context.CreateUser("original", GameUserRole.User);
        GameUser target = context.CreateUser("stinker", GameUserRole.User);
        
        // Ensure we're tracking neither usernames
        Assert.That(!context.Database.WasUsernamePreviouslyTaken("original"));
        Assert.That(!context.Database.WasUsernamePreviouslyTaken("stinker"));

        context.Database.RenameUser(owner, "original_2");
        GameUser? modifiedOwner = context.Database.GetUserByObjectId(owner.UserId);
        Assert.That(modifiedOwner, Is.Not.Null);
        Assert.That(modifiedOwner!.Username, Is.EqualTo("original_2"));

        // Try to rename stinker to original's previous name
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, mod);
        ApiAdminUpdateUserRequest request = new()
        {
            Username = "original"
        };

        ApiResponse<ApiExtendedGameUserResponse>? response = client.PatchData<ApiExtendedGameUserResponse>($"/api/v3/admin/users/uuid/{target.UserId}", request, true, false);
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.Username, Is.EqualTo("original"));
        Assert.That(response!.Data!.UserId, Is.EqualTo(target.UserId.ToString()));

        context.Database.Refresh();

        GameUser? modifiedTarget = context.Database.GetUserByObjectId(target.UserId);
        Assert.That(modifiedTarget, Is.Not.Null);
        Assert.That(modifiedTarget!.Username, Is.EqualTo("original"));
        
        // Ensure we're tracking both usernames
        Assert.That(context.Database.WasUsernamePreviouslyTaken("original"));
        Assert.That(context.Database.WasUsernamePreviouslyTaken("stinker"));
        
        // Ensure "original" is tracked as previously owned by owner
        DatabaseList<PreviousUsername> originalHistory = context.Database.GetPreviousUsernameRecordsByName("original", 0, 10);
        Assert.That(originalHistory.Items.Count, Is.EqualTo(1));
        Assert.That(originalHistory.Items.First().UserId.ToString(), Is.EqualTo(owner.UserId.ToString()));
        
        // Ensure "stinker" is tracked as previously owned by target
        DatabaseList<PreviousUsername> stinkerHistory = context.Database.GetPreviousUsernameRecordsByName("stinker", 0, 10);
        Assert.That(stinkerHistory.Items.Count, Is.EqualTo(1));
        Assert.That(stinkerHistory.Items.First().UserId.ToString(), Is.EqualTo(target.UserId.ToString()));
    }

    [Test]
    public void CanRenameUserBackToTheirOwnPreviousName()
    {
        using TestContext context = this.GetServer();

        GameUser mod = context.CreateUser(null, GameUserRole.Moderator);
        GameUser owner = context.CreateUser("original", GameUserRole.User);

        context.Database.RenameUser(owner, "original_2");
        GameUser? modifiedOwner = context.Database.GetUserByObjectId(owner.UserId);
        Assert.That(modifiedOwner, Is.Not.Null);
        Assert.That(modifiedOwner!.Username, Is.EqualTo("original_2"));

        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, mod);
        ApiAdminUpdateUserRequest request = new()
        {
            Username = "original"
        };

        ApiResponse<ApiExtendedGameUserResponse>? response = client.PatchData<ApiExtendedGameUserResponse>($"/api/v3/admin/users/uuid/{owner.UserId}", request);
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.Username == "original");

        context.Database.Refresh();

        GameUser? modifiedOwner2 = context.Database.GetUserByObjectId(owner.UserId);
        Assert.That(modifiedOwner2, Is.Not.Null);
        Assert.That(modifiedOwner2!.Username, Is.EqualTo("original"));
        
        // Ensure "original" is still also tracked as previously owned by owner
        DatabaseList<PreviousUsername> originalHistory = context.Database.GetPreviousUsernameRecordsByName("original", 0, 10);
        Assert.That(originalHistory.Items.Count, Is.EqualTo(1));
        Assert.That(originalHistory.Items.First().UserId.ToString(), Is.EqualTo(owner.UserId.ToString()));
    }
    
    [Test]
    public void PreviousUsernameAdminEndpointsRequireAuth()
    {
        using TestContext context = this.GetServer();
        GameUser target = context.CreateUser("theName");
        
        // test with at least one actual rename
        context.Database.RenameUser(target, "theCoolerName");
        
        // cannot access
        HttpResponseMessage response = context.Http.GetAsync($"/api/v3/admin/previousUsernames/byUser/uuid/{target.UserId}").Result;
        Assert.That(response.StatusCode, Is.EqualTo(Forbidden));
        
        response = context.Http.GetAsync($"/api/v3/admin/previousUsernames/byUser/name/{target.Username}").Result;
        Assert.That(response.StatusCode, Is.EqualTo(Forbidden));
        
        response = context.Http.GetAsync($"/api/v3/admin/previousUsernames/byName/theName").Result;
        Assert.That(response.StatusCode, Is.EqualTo(Forbidden));
    }
    
    [Test]
    [TestCase(GameUserRole.Restricted, false)]
    [TestCase(GameUserRole.User, false)]
    [TestCase(GameUserRole.Trusted, false)]
    [TestCase(GameUserRole.Curator, false)]
    [TestCase(GameUserRole.Moderator, true)]
    [TestCase(GameUserRole.Admin, true)]
    public void PreviousUsernameAdminEndpointsAreGuardedByRole(GameUserRole accessorRole, bool mayAccess)
    {
        using TestContext context = this.GetServer();
        GameUser accessor = context.CreateUser("accessor", accessorRole);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, accessor);
        
        // Prepare
        GameUser target1 = context.CreateUser("coolName1");
        GameUser target2 = context.CreateUser("coolName2");
        context.Database.RenameUser(target1, "rename1");
        context.Database.RenameUser(target2, "rename2");
        
        Action<ApiListResponse<ApiExtendedPreviousUsernameResponse>?, ApiListResponse<ApiExtendedPreviousUsernameResponse>?> assertionCB 
            = delegate(ApiListResponse<ApiExtendedPreviousUsernameResponse>? response1, ApiListResponse<ApiExtendedPreviousUsernameResponse>? response2)
            {
                if (mayAccess)
                {
                    Assert.That(response1?.Data, Is.Not.Null);
                    Assert.That(response1?.ListInfo, Is.Not.Null);
                
                    Assert.That(response1!.Data!.Count, Is.EqualTo(1));
                    Assert.That(response1!.ListInfo!.TotalItems, Is.EqualTo(1));
                
                    Assert.That(response1.Data!.First().Username, Is.EqualTo("coolName1"));
                    Assert.That(response1.Data!.First().User.Username, Is.EqualTo("rename1"));
                    Assert.That(response1.Data!.First().User.UserId.ToString(), Is.EqualTo(target1.UserId.ToString()));
                
                    Assert.That(response2?.Data, Is.Not.Null);
                    Assert.That(response2?.ListInfo, Is.Not.Null);
                
                    Assert.That(response2!.Data!.Count, Is.EqualTo(1));
                    Assert.That(response2!.ListInfo!.TotalItems, Is.EqualTo(1));
                
                    Assert.That(response2.Data!.First().Username, Is.EqualTo("coolName2"));
                    Assert.That(response2.Data!.First().User.Username, Is.EqualTo("rename2"));
                    Assert.That(response2.Data!.First().User.UserId.ToString(), Is.EqualTo(target2.UserId.ToString()));
                }
                else
                {
                    Assert.That(response1, Is.Null);
                    Assert.That(response2, Is.Null);
                }
            };
        
        // Now test
        ApiListResponse<ApiExtendedPreviousUsernameResponse>? response1 = client.GetList<ApiExtendedPreviousUsernameResponse>(
            $"/api/v3/admin/previousUsernames/byUser/uuid/{target1.UserId}", mayAccess, !mayAccess);
        ApiListResponse<ApiExtendedPreviousUsernameResponse>? response2 = client.GetList<ApiExtendedPreviousUsernameResponse>(
            $"/api/v3/admin/previousUsernames/byUser/uuid/{target2.UserId}", mayAccess, !mayAccess);
        assertionCB(response1, response2);
        
        ApiListResponse<ApiExtendedPreviousUsernameResponse>? response3 = client.GetList<ApiExtendedPreviousUsernameResponse>(
            $"/api/v3/admin/previousUsernames/byUser/name/{target1.Username}", mayAccess, !mayAccess);
        ApiListResponse<ApiExtendedPreviousUsernameResponse>? response4 = client.GetList<ApiExtendedPreviousUsernameResponse>(
            $"/api/v3/admin/previousUsernames/byUser/name/{target2.Username}", mayAccess, !mayAccess);
        assertionCB(response3, response4);
        
        ApiListResponse<ApiExtendedPreviousUsernameResponse>? response5 = client.GetList<ApiExtendedPreviousUsernameResponse>(
            $"/api/v3/admin/previousUsernames/byName/coolName1", mayAccess, !mayAccess);
        ApiListResponse<ApiExtendedPreviousUsernameResponse>? response6 = client.GetList<ApiExtendedPreviousUsernameResponse>(
            $"/api/v3/admin/previousUsernames/byName/coolName2", mayAccess, !mayAccess);
        assertionCB(response5, response6);
    }

    [Test]
    public void CanRenameUserBackAndForth()
    {
        using TestContext context = this.GetServer();

        GameUser mod = context.CreateUser(null, GameUserRole.Moderator);
        GameUser owner = context.CreateUser("original", GameUserRole.User);

        context.Database.RenameUser(owner, "original_2");
        GameUser? modifiedOwner = context.Database.GetUserByObjectId(owner.UserId);
        Assert.That(modifiedOwner, Is.Not.Null);
        Assert.That(modifiedOwner!.Username, Is.EqualTo("original_2"));

        context.Time.TimestampMilliseconds += 1000;

        context.Database.RenameUser(owner, "original");
        modifiedOwner = context.Database.GetUserByObjectId(owner.UserId);
        Assert.That(modifiedOwner, Is.Not.Null);
        Assert.That(modifiedOwner!.Username, Is.EqualTo("original"));

        context.Time.TimestampMilliseconds += 1000;

        context.Database.RenameUser(owner, "original_2");
        modifiedOwner = context.Database.GetUserByObjectId(owner.UserId);
        Assert.That(modifiedOwner, Is.Not.Null);
        Assert.That(modifiedOwner!.Username, Is.EqualTo("original_2"));

        context.Time.TimestampMilliseconds += 1000;

        context.Database.RenameUser(owner, "original");
        modifiedOwner = context.Database.GetUserByObjectId(owner.UserId);
        Assert.That(modifiedOwner, Is.Not.Null);
        Assert.That(modifiedOwner!.Username, Is.EqualTo("original"));
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

    [Test]
    public void ModeratorsMayNotUpdateMetadataOfAdminsAndModerators()
    {
        using TestContext context = this.GetServer();
        GameUser admin = context.CreateUser(role: GameUserRole.Admin);
        GameUser mod = context.CreateUser(role: GameUserRole.Moderator);
        GameUser mod2 = context.CreateUser(role: GameUserRole.Moderator);
        GameUser user = context.CreateUser(role: GameUserRole.User);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, mod);
        ApiAdminUpdateUserRequest request = new()
        {
            Username = "hahahalol",
            Description = "pee"
        };

        // Admin
        ApiResponse<ApiExtendedGameUserResponse>? response = client.PatchData<ApiExtendedGameUserResponse>($"/api/v3/admin/users/uuid/{admin.UserId}", request, false, true);
        Assert.That(response?.Error, Is.Not.Null);
        Assert.That(response!.Error!.StatusCode, Is.EqualTo(BadRequest));
        context.Database.Refresh();

        GameUser? updated = context.Database.GetUserByObjectId(admin.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Username, Is.Not.EqualTo(request.Username));
        Assert.That(updated!.Description, Is.Not.EqualTo(request.Description));

        // Mod
        response = client.PatchData<ApiExtendedGameUserResponse>($"/api/v3/admin/users/uuid/{mod2.UserId}", request, false, true);
        Assert.That(response?.Error, Is.Not.Null);
        Assert.That(response!.Error!.StatusCode, Is.EqualTo(BadRequest));
        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(mod2.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Username, Is.Not.EqualTo(request.Username));
        Assert.That(updated!.Description, Is.Not.EqualTo(request.Description));

        // User
        response = client.PatchData<ApiExtendedGameUserResponse>($"/api/v3/admin/users/uuid/{user.UserId}", request, true, false);
        Assert.That(response?.Data, Is.Not.Null);
        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Username, Is.EqualTo(request.Username));
        Assert.That(updated!.Description, Is.EqualTo(request.Description));
    }
}