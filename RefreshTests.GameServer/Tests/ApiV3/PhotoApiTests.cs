using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using RefreshTests.GameServer.Extensions;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users.Photos;
using Refresh.Database.Models.Authentication;

namespace RefreshTests.GameServer.Tests.ApiV3;

public class PhotoApiTests : GameServerTest
{
    private const string TEST_IMAGE_HASH = "0ec63b140374ba704a58fa0c743cb357683313dd";

    [Test]
    public void GetsPhotosByUserUuidAndName()
    {
        using TestContext context = this.GetServer();
        GameUser player = context.CreateUser();
        context.CreatePhotoWithSubject(player, TEST_IMAGE_HASH);
        
        ApiListResponse<ApiGamePhotoResponse>? response = context.Http.GetList<ApiGamePhotoResponse>($"/api/v3/photos/by/uuid/{player.UserId}");
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.Count, Is.EqualTo(1));

        response = context.Http.GetList<ApiGamePhotoResponse>($"/api/v3/photos/by/name/{player.Username}");
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.Count, Is.EqualTo(1));
    }

    [Test]
    public void GetsPhotosWithUserUuidAndName()
    {
        using TestContext context = this.GetServer();
        GameUser player = context.CreateUser();
        context.CreatePhotoWithSubject(player, TEST_IMAGE_HASH);
        
        ApiListResponse<ApiGamePhotoResponse>? response = context.Http.GetList<ApiGamePhotoResponse>($"/api/v3/photos/with/uuid/{player.UserId}");
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.Count, Is.EqualTo(1));

        response = context.Http.GetList<ApiGamePhotoResponse>($"/api/v3/photos/with/name/{player.Username}");
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task DeletesAllPhotosByUserUuidAndName()
    {
        using TestContext context = this.GetServer();
        GameUser player = context.CreateUser(role: GameUserRole.User);
        GameUser mod = context.CreateUser(role: GameUserRole.Moderator);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, mod);

        // UUID
        context.CreatePhotoWithSubject(player, TEST_IMAGE_HASH);
        HttpResponseMessage resetResponse = await client.DeleteAsync($"/api/v3/admin/users/uuid/{player.UserId}/photos");
        Assert.That(resetResponse.IsSuccessStatusCode, Is.True);
        Assert.That(context.Database.GetTotalPhotosByUser(player), Is.Zero);

        // name
        context.CreatePhotoWithSubject(player, TEST_IMAGE_HASH);
        resetResponse = await client.DeleteAsync($"/api/v3/admin/users/name/{player.Username}/photos");
        Assert.That(resetResponse.IsSuccessStatusCode, Is.True);
        Assert.That(context.Database.GetTotalPhotosByUser(player), Is.Zero);
    }
}