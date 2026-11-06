using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using RefreshTests.GameServer.Extensions;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request.Authentication;
using Refresh.Common.Helpers;
using System.Security.Cryptography;
using Refresh.Interfaces.Game.Types.UserData;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Admin;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Common.Extensions;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;

namespace RefreshTests.GameServer.Tests.ApiV3;

public class AdminUserManagementApiTests : GameServerTest
{
    [Test]
    public async Task ResetsUsersPasswordByUuidAndName()
    {
        using TestContext context = this.GetServer();
        GameUser mod = context.CreateUser(role: GameUserRole.Moderator);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, mod);

        // UUID
        GameUser player1 = context.CreateUser(role: GameUserRole.User);
        ApiResetPasswordRequest request = new()
        {
            PasswordSha512 = HexHelper.BytesToHexString(SHA512.HashData("poo"u8))
        };
        HttpResponseMessage response = await client.PutAsync($"/api/v3/admin/users/uuid/{player1.UserId}/resetPassword", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.True);

        context.Database.Refresh();
        GameUser? updated1 = context.Database.GetUserByObjectId(player1.UserId);
        Assert.That(updated1, Is.Not.Null);
        Assert.That(updated1!.ShouldResetPassword, Is.True);

        // Name
        GameUser player2 = context.CreateUser(role: GameUserRole.User);
        request = new()
        {
            PasswordSha512 = HexHelper.BytesToHexString(SHA512.HashData("poo"u8))
        };
        response = await client.PutAsync($"/api/v3/admin/users/name/{player2.Username}/resetPassword", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.True);

        context.Database.Refresh();
        GameUser? updated2 = context.Database.GetUserByObjectId(player2.UserId);
        Assert.That(updated2, Is.Not.Null);
        Assert.That(updated2!.ShouldResetPassword, Is.True);
    }

    [Test]
    public async Task GetsAndResetsUserPlanetsByUuidAndName()
    {
        using TestContext context = this.GetServer();
        GameUser mod = context.CreateUser(role: GameUserRole.Moderator);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, mod);
        GameUser player = context.CreateUser(role: GameUserRole.User);

        // UUID
        context.Database.UpdateUserData(player, new SerializedUpdateData()
        {
            PlanetsHash = "lol"
        }, TokenGame.LittleBigPlanet2);
        
        ApiResponse<ApiAdminUserPlanetsResponse>? planetResponse = client.GetData<ApiAdminUserPlanetsResponse>($"/api/v3/admin/users/uuid/{player.UserId}/planets");
        Assert.That(planetResponse?.Data, Is.Not.Null);
        Assert.That(planetResponse!.Data!.Lbp2PlanetsHash, Is.EqualTo("lol"));

        HttpResponseMessage resetResponse = await client.DeleteAsync($"/api/v3/admin/users/uuid/{player.UserId}/planets");
        Assert.That(resetResponse.IsSuccessStatusCode, Is.True);

        context.Database.Refresh();

        GameUser? updated = context.Database.GetUserByObjectId(player.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Lbp2PlanetsHash.IsBlankHash(), Is.True);
        
        // name
        context.Database.UpdateUserData(updated, new SerializedUpdateData()
        {
            PlanetsHash = "lol"
        }, TokenGame.LittleBigPlanet3);

        context.Database.Refresh();

        planetResponse = client.GetData<ApiAdminUserPlanetsResponse>($"/api/v3/admin/users/name/{updated.Username}/planets");
        Assert.That(planetResponse?.Data, Is.Not.Null);
        Assert.That(planetResponse!.Data!.Lbp3PlanetsHash, Is.EqualTo("lol"));

        resetResponse = await client.DeleteAsync($"/api/v3/admin/users/name/{updated.Username}/planets");
        Assert.That(resetResponse.IsSuccessStatusCode, Is.True);

        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(updated.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Lbp3PlanetsHash.IsBlankHash(), Is.True);
    }

    [Test]
    public async Task ModeratorsMayNotDeleteAdminsAndModerators()
    {
        using TestContext context = this.GetServer();
        GameUser admin = context.CreateUser(role: GameUserRole.Admin);
        GameUser mod = context.CreateUser(role: GameUserRole.Moderator);
        GameUser mod2 = context.CreateUser(role: GameUserRole.Moderator);
        GameUser user = context.CreateUser(role: GameUserRole.User);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, mod);

        // Admin
        HttpResponseMessage response = await client.DeleteAsync($"/api/v3/admin/users/uuid/{admin.UserId}");
        Assert.That(response.IsSuccessStatusCode, Is.False);
        context.Database.Refresh();

        GameUser? updated = context.Database.GetUserByObjectId(admin.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Role, Is.EqualTo(GameUserRole.Admin));

        // Mod
        response = await client.DeleteAsync($"/api/v3/admin/users/uuid/{mod2.UserId}");
        Assert.That(response.IsSuccessStatusCode, Is.False);
        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(mod2.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Role, Is.EqualTo(GameUserRole.Moderator));

        // User
        response = await client.DeleteAsync($"/api/v3/admin/users/uuid/{user.UserId}");
        Assert.That(response.IsSuccessStatusCode, Is.True);
        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Role, Is.EqualTo(GameUserRole.Banned));
    }

    [Test]
    public async Task ModeratorsMayNotBanAdminsAndModerators()
    {
        using TestContext context = this.GetServer();
        GameUser admin = context.CreateUser(role: GameUserRole.Admin);
        GameUser mod = context.CreateUser(role: GameUserRole.Moderator);
        GameUser mod2 = context.CreateUser(role: GameUserRole.Moderator);
        GameUser user = context.CreateUser(role: GameUserRole.User);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, mod);
        ApiPunishUserRequest request = new()
        {
            Reason = "lol",
            ExpiryDate = new(2036, 8, 12, 4, 20, 9, 213, new())
        };

        // Admin
        HttpResponseMessage response = await client.PostAsync($"/api/v3/admin/users/uuid/{admin.UserId}/ban", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.False);
        context.Database.Refresh();

        GameUser? updated = context.Database.GetUserByObjectId(admin.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Role, Is.EqualTo(GameUserRole.Admin));

        // Mod
        response = await client.PostAsync($"/api/v3/admin/users/uuid/{mod2.UserId}/ban", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.False);
        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(mod2.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Role, Is.EqualTo(GameUserRole.Moderator));

        // User
        response = await client.PostAsync($"/api/v3/admin/users/uuid/{user.UserId}/ban", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.True);
        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Role, Is.EqualTo(GameUserRole.Banned));
    }

    [Test]
    public async Task ModeratorsMayNotRestrictAdminsAndModerators()
    {
        using TestContext context = this.GetServer();
        GameUser admin = context.CreateUser(role: GameUserRole.Admin);
        GameUser mod = context.CreateUser(role: GameUserRole.Moderator);
        GameUser mod2 = context.CreateUser(role: GameUserRole.Moderator);
        GameUser user = context.CreateUser(role: GameUserRole.User);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, mod);
        ApiPunishUserRequest request = new()
        {
            Reason = "lol",
            ExpiryDate = new(2036, 8, 12, 4, 20, 9, 213, new())
        };

        // Admin
        HttpResponseMessage response = await client.PostAsync($"/api/v3/admin/users/uuid/{admin.UserId}/restrict", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.False);
        context.Database.Refresh();

        GameUser? updated = context.Database.GetUserByObjectId(admin.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Role, Is.EqualTo(GameUserRole.Admin));

        // Mod
        response = await client.PostAsync($"/api/v3/admin/users/uuid/{mod2.UserId}/restrict", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.False);
        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(mod2.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Role, Is.EqualTo(GameUserRole.Moderator));

        // User
        response = await client.PostAsync($"/api/v3/admin/users/uuid/{user.UserId}/restrict", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.True);
        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Role, Is.EqualTo(GameUserRole.Restricted));
    }

    [Test]
    public async Task CanOnlyPardonPunishedUsers()
    {
        using TestContext context = this.GetServer();
        GameUser admin = context.CreateUser(role: GameUserRole.Admin);
        GameUser mod = context.CreateUser(role: GameUserRole.Moderator);
        GameUser user = context.CreateUser(role: GameUserRole.User);
        GameUser restricted = context.CreateUser(role: GameUserRole.Restricted);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, mod);

        // Admin
        HttpResponseMessage response = await client.PostAsync($"/api/v3/admin/users/uuid/{admin.UserId}/pardon", null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        context.Database.Refresh();

        GameUser? updated = context.Database.GetUserByObjectId(admin.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Role, Is.EqualTo(GameUserRole.Admin));

        // User
        response = await client.PostAsync($"/api/v3/admin/users/uuid/{user.UserId}/pardon", null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Role, Is.EqualTo(GameUserRole.User));

        // Restricted
        response = await client.PostAsync($"/api/v3/admin/users/uuid/{restricted.UserId}/pardon", null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(restricted.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Role, Is.EqualTo(GameUserRole.User));
    }

    [Test]
    public async Task ModeratorsMayNotResetPasswordOfAdminsAndModerators()
    {
        using TestContext context = this.GetServer();
        GameUser admin = context.CreateUser(role: GameUserRole.Admin);
        GameUser mod = context.CreateUser(role: GameUserRole.Moderator);
        GameUser mod2 = context.CreateUser(role: GameUserRole.Moderator);
        GameUser user = context.CreateUser(role: GameUserRole.User);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, mod);
        ApiResetPasswordRequest request = new()
        {
            PasswordSha512 = HexHelper.BytesToHexString(SHA512.HashData("lmao"u8))
        };

        // Admin
        HttpResponseMessage response = await client.PutAsync($"/api/v3/admin/users/uuid/{admin.UserId}/resetPassword", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.False);
        context.Database.Refresh();

        GameUser? updated = context.Database.GetUserByObjectId(admin.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.ShouldResetPassword, Is.False);

        // Mod
        response = await client.PutAsync($"/api/v3/admin/users/uuid/{mod2.UserId}/resetPassword", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.False);
        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(mod2.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.ShouldResetPassword, Is.False);

        // User
        response = await client.PutAsync($"/api/v3/admin/users/uuid/{user.UserId}/resetPassword", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.True);
        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.ShouldResetPassword, Is.True);
    }

    [Test]
    public async Task ModeratorsMayEditUserEmailAddress()
    {
        using TestContext context = this.GetServer();
        GameUser mod = context.CreateUser(role: GameUserRole.Moderator, verifyEmail: true);
        GameUser user = context.CreateUser(role: GameUserRole.User);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, mod);
        ApiAdminUpdateEmailAddressRequest request = new()
        {
            EmailAddress = "lol@lol.lol",
        };

        // --- Edit just the address ---
        HttpResponseMessage response = await client.PutAsync($"/api/v3/admin/users/uuid/{user.UserId}/emailAddress", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.True);
        context.Database.Refresh();

        GameUser? updated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.EmailAddress, Is.EqualTo("lol@lol.lol"));
        Assert.That(updated!.EmailAddressVerified, Is.False); // false again, even though we've explicitly set to true when creating

        // --- Can verify manually ---
        request = new()
        {
            MarkEmailAddressAsVerified = true
        };
        response = await client.PutAsync($"/api/v3/admin/users/uuid/{user.UserId}/emailAddress", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.True);
        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.EmailAddress, Is.EqualTo("lol@lol.lol"));
        Assert.That(updated!.EmailAddressVerified, Is.True);

        // --- Can also unverify manually ---
        request = new()
        {
            MarkEmailAddressAsVerified = false,
        };
        response = await client.PutAsync($"/api/v3/admin/users/uuid/{user.UserId}/emailAddress", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.True);
        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.EmailAddress, Is.EqualTo("lol@lol.lol"));
        Assert.That(updated!.EmailAddressVerified, Is.False);

        // --- Can also do all of it ---
        request = new()
        {
            EmailAddress = "lel@lel.lel",
            MarkEmailAddressAsVerified = true,
            Reason = "garbage email"
        };
        response = await client.PutAsync($"/api/v3/admin/users/uuid/{user.UserId}/emailAddress", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.True);
        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.EmailAddress, Is.EqualTo("lel@lel.lel"));
        Assert.That(updated!.EmailAddressVerified, Is.True);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task TestInvalidEmailAddressesGivenByModerators(bool force)
    {
        using TestContext context = this.GetServer();
        GameUser mod = context.CreateUser(role: GameUserRole.Moderator, verifyEmail: true);
        GameUser user = context.CreateUser(role: GameUserRole.User);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, mod);

        context.Database.UpdateUserEmailAddress(user, "init@real.real", true, true);

        // --- May only set to malformed if forced ---
        ApiAdminUpdateEmailAddressRequest request = new()
        {
            EmailAddress = "malformed lol",
            Force = force,
        };

        HttpResponseMessage response = await client.PutAsync($"/api/v3/admin/users/uuid/{user.UserId}/emailAddress", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(force));
        context.Database.Refresh();

        GameUser? updated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.EmailAddress, Is.EqualTo(force ? "lol@lol.lol" : "init@real.real"));

        // false if forcefully changed, true if not forcefully changed (since update was rejected)
        Assert.That(updated!.EmailAddressVerified, Is.EqualTo(!force));

        // --- May only set to disallowed if forced ---
        context.Database.DisallowEmailAddress("disallowed lol", "reason");
        request = new()
        {
            EmailAddress = "disallowed@real.real",
        };
        response = await client.PutAsync($"/api/v3/admin/users/uuid/{user.UserId}/emailAddress", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(force));
        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.EmailAddress, Is.EqualTo(force ? "disallowed@real.real" : "init@real.real"));

        // false if forcefully changed, true if not forcefully changed (since update was rejected)
        Assert.That(updated!.EmailAddressVerified, Is.EqualTo(!force));

        // --- May only set to taken (existing user) if forced ---
        context.Database.CreateUser("lol", "taken@real.real", true);
        request = new()
        {
            EmailAddress = "taken@real.real",
        };
        response = await client.PutAsync($"/api/v3/admin/users/uuid/{user.UserId}/emailAddress", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(force));
        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.EmailAddress, Is.EqualTo(force ? "taken@real.real" : "init@real.real"));

        // false if forcefully changed, true if not forcefully changed (since update was rejected)
        Assert.That(updated!.EmailAddressVerified, Is.EqualTo(!force));

        // --- May only set to taken (queued registration) if forced ---
        context.Database.AddRegistrationToQueue("lel", "queue@real.real", "very secure");
        request = new()
        {
            EmailAddress = "queue@real.real",
        };
        response = await client.PutAsync($"/api/v3/admin/users/uuid/{user.UserId}/emailAddress", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(force));
        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.EmailAddress, Is.EqualTo(force ? "queue@real.real" : "init@real.real"));

        // false if forcefully changed, true if not forcefully changed (since update was rejected)
        Assert.That(updated!.EmailAddressVerified, Is.EqualTo(!force));
    }

    [Test]
    public async Task ModeratorsMayEditUsernames()
    {
        using TestContext context = this.GetServer();
        GameUser mod = context.CreateUser("Init", role: GameUserRole.Moderator, verifyEmail: true);
        GameUser user = context.CreateUser(role: GameUserRole.User);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, mod);
        ApiAdminRenameUserRequest request = new()
        {
            Username = "Changed",
        };

        // --- Edit just the name ---
        HttpResponseMessage response = await client.PutAsync($"/api/v3/admin/users/uuid/{user.UserId}/username", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.True);
        context.Database.Refresh();

        GameUser? updated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Username, Is.EqualTo("Changed"));

        // --- Rename and disallow old name ---
        request = new()
        {
            Username = "Hidden",
            Reason = "Cringe",
        };
        response = await client.PutAsync($"/api/v3/admin/users/uuid/{user.UserId}/username", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.True);
        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Username, Is.EqualTo("Hidden"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task TestInvalidUsernamesGivenByModerators(bool force)
    {
        using TestContext context = this.GetServer();
        GameUser mod = context.CreateUser("Init", role: GameUserRole.Moderator, verifyEmail: true);
        GameUser user = context.CreateUser(role: GameUserRole.User);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, mod);

        // --- May only set to malformed if forced ---
        ApiAdminRenameUserRequest request = new()
        {
            Username = "d",
            Force = force,
        };

        HttpResponseMessage response = await client.PutAsync($"/api/v3/admin/users/uuid/{user.UserId}/username", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(force));
        context.Database.Refresh();

        GameUser? updated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Username, Is.EqualTo(force ? "d" : "Init"));

        // --- May only set to disallowed if forced ---
        context.Database.DisallowUser("SuperDisallowed", "reason");
        request = new()
        {
            Username = "SuperDisallowed",
        };
        response = await client.PutAsync($"/api/v3/admin/users/uuid/{user.UserId}/username", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(force));
        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Username, Is.EqualTo(force ? "SuperDisallowed" : "Init"));

        // --- May only set to taken (existing user) if forced ---
        context.Database.CreateUser("Taken", "taken@real.real", true);
        request = new()
        {
            Username = "Taken",
        };
        response = await client.PutAsync($"/api/v3/admin/users/uuid/{user.UserId}/username", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(force));
        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Username, Is.EqualTo(force ? "Taken" : "Init"));

        // --- May only set to taken (queued registration) if forced ---
        context.Database.AddRegistrationToQueue("Queued", "queue@real.real", "very secure");
        request = new()
        {
            Username = "Queued",
        };
        response = await client.PutAsync($"/api/v3/admin/users/uuid/{user.UserId}/username", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.EqualTo(force));
        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Username, Is.EqualTo(force ? "Queued" : "Init"));
    }
}