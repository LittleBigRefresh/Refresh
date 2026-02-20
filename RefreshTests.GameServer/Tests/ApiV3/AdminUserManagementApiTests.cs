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

namespace RefreshTests.GameServer.Tests.ApiV3;

public class AdminUserManagementApiTests : GameServerTest
{
    [Test]
    public async Task ResetsUsersPasswordByUuidAndName()
    {
        using TestContext context = this.GetServer();
        GameUser mod = context.CreateUser(role: GameUserRole.Moderator);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, mod);

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

        GameUser player2 = context.CreateUser(role: GameUserRole.User);
        request = new()
        {
            PasswordSha512 = HexHelper.BytesToHexString(SHA512.HashData("poo"u8))
        };
        response = await client.PutAsync($"/api/v3/admin/users/uuid/{player2.UserId}/resetPassword", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.True);

        context.Database.Refresh();
        GameUser? updated2 = context.Database.GetUserByObjectId(player2.UserId);
        Assert.That(updated2, Is.Not.Null);
        Assert.That(updated2!.ShouldResetPassword, Is.True);

        GameUser player3 = context.CreateUser(role: GameUserRole.User);
        request = new()
        {
            PasswordSha512 = HexHelper.BytesToHexString(SHA512.HashData("poo"u8))
        };
        response = await client.PutAsync($"/api/v3/admin/users/uuid/{player3.UserId}/resetPassword", new StringContent(request.AsJson()));
        Assert.That(response.IsSuccessStatusCode, Is.True);

        context.Database.Refresh();
        GameUser? updated3 = context.Database.GetUserByObjectId(player3.UserId);
        Assert.That(updated3, Is.Not.Null);
        Assert.That(updated3!.ShouldResetPassword, Is.True);
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

        // username
        context.Database.UpdateUserData(updated, new SerializedUpdateData()
        {
            PlanetsHash = "lol"
        }, TokenGame.LittleBigPlanetVita);

        context.Database.Refresh();

        planetResponse = client.GetData<ApiAdminUserPlanetsResponse>($"/api/v3/admin/users/username/{updated.Username}/planets");
        Assert.That(planetResponse?.Data, Is.Not.Null);
        Assert.That(planetResponse!.Data!.VitaPlanetsHash, Is.EqualTo("lol"));

        resetResponse = await client.DeleteAsync($"/api/v3/admin/users/username/{updated.Username}/planets");
        Assert.That(resetResponse.IsSuccessStatusCode, Is.True);

        context.Database.Refresh();

        updated = context.Database.GetUserByObjectId(updated.UserId);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.VitaPlanetsHash.IsBlankHash(), Is.True);
    }
}