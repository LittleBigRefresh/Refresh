using System.Security.Cryptography;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes.Errors;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Data;
using Refresh.Interfaces.Game.Types.UserData;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Assets;

public class AssetDisallowanceTests : GameServerTest
{
    [Test]
    public void CanDisallowAndReallowAsset()
    {
        using TestContext context = this.GetServer();
        
        string hash = "trash";
        GameAssetType type = GameAssetType.Mesh;

        // Ensure that the asset isn't already disallowed
        Assert.That(context.Database.GetDisallowedAssetInfo(hash), Is.Null);

        // Disallow
        (DisallowedAsset disallowed, bool success) = context.Database.DisallowAsset(hash, type, "too ugly");
        Assert.That(success, Is.True);
        Assert.That(disallowed.AssetHash, Is.EqualTo(hash));
        Assert.That(disallowed.AssetType, Is.EqualTo(type));
        Assert.That(disallowed.Reason, Is.EqualTo("too ugly"));

        // Ensure that the same entity is gotten again, and the DB method doesn't try to insert a new one
        (disallowed, success) = context.Database.DisallowAsset(hash, type, "too ugly");
        Assert.That(success, Is.False);
        Assert.That(disallowed.AssetHash, Is.EqualTo(hash));
        Assert.That(disallowed.AssetType, Is.EqualTo(type));
        Assert.That(disallowed.Reason, Is.EqualTo("too ugly"));

        // ensure that the separately gotten entity is also the same
        DisallowedAsset? gottenAgain = context.Database.GetDisallowedAssetInfo(hash);
        Assert.That(gottenAgain, Is.Not.Null);
        Assert.That(success, Is.False);
        Assert.That(disallowed.AssetHash, Is.EqualTo(hash));
        Assert.That(disallowed.AssetType, Is.EqualTo(type));
        Assert.That(disallowed.Reason, Is.EqualTo("too ugly"));

        // Ensure it doesn't also return this if the hash is different
        Assert.That(context.Database.GetDisallowedAssetInfo("bash"), Is.Null);

        // Reallow
        success = context.Database.ReallowAsset(hash);
        Assert.That(success, Is.True);
        Assert.That(context.Database.GetDisallowedAssetInfo(hash), Is.Null);

        // Reallow again
        success = context.Database.ReallowAsset(hash);
        Assert.That(success, Is.False);
        Assert.That(context.Database.GetDisallowedAssetInfo(hash), Is.Null);
    }

    [Test]
    public void ShowModeratedReturnsDisallowedAssetHashes()
    {
        using TestContext context = this.GetServer();
        GameAsset one = new()
        {
            AssetHash = "1",
            AssetType = GameAssetType.Texture,
            AssetFormat = GameAssetFormat.Binary,
        };
        GameAsset two = new()
        {
            AssetHash = "2",
            AssetType = GameAssetType.Texture,
            AssetFormat = GameAssetFormat.Binary,
        };
        GameAsset three = new()
        {
            AssetHash = "3",
            AssetType = GameAssetType.Texture,
            AssetFormat = GameAssetFormat.Binary,
        };
        GameAsset four = new()
        {
            AssetHash = "4",
            AssetType = GameAssetType.Texture,
            AssetFormat = GameAssetFormat.Binary,
        };

        context.Database.AddAssetsToDatabase([one, two, three, four]);
        GameUser user = context.CreateUser();
        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        SerializedModeratedResourceList request = new()
        {
            Resources = ["1", "2", "4", "6"] // also test against assets not in DB (in this case "6")
        };
        HttpResponseMessage message = client.PostAsync("/lbp/showModerated", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedModeratedResourceList response = message.Content.ReadAsXML<SerializedModeratedResourceList>();
        Assert.That(response.Resources, Is.Empty);

        // Now disallow a few assets and try again
        context.Database.DisallowAsset("3", GameAssetType.Texture, "Cringe drawing");
        context.Database.DisallowAsset("7", GameAssetType.Plan, "Cringe drawing");
        context.Database.DisallowAsset("9", GameAssetType.Plan, "Cringe drawing");

        request = new()
        {
            Resources = ["1", "3", "4", "6", "7"]
        };
        message = client.PostAsync("/lbp/showModerated", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        response = message.Content.ReadAsXML<SerializedModeratedResourceList>();
        Assert.That(response.Resources.Count, Is.EqualTo(2));
        Assert.That(response.Resources.Contains("3"), Is.True);
        Assert.That(response.Resources.Contains("7"), Is.True);
    }

    [Test]
    public void CannotUploadDisallowedAssetFromGame()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        ReadOnlySpan<byte> data = "TEX a"u8;
        
        string hash = BitConverter.ToString(SHA1.HashData(data))
            .Replace("-", "")
            .ToLower();

        context.Database.DisallowAsset(hash, GameAssetType.Texture, "Weegee");

        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{hash}", new ByteArrayContent(data.ToArray())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(Unauthorized));
    }

    [Test]
    public void CannotUploadDisallowedAssetFromApi()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);

        ReadOnlySpan<byte> data = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        
        string hash = BitConverter.ToString(SHA1.HashData(data))
            .Replace("-", "")
            .ToLower();

        context.Database.DisallowAsset(hash, GameAssetType.Png, "Weegee");

        ApiResponse<ApiGameAssetResponse>? response = client.PostData<ApiGameAssetResponse>($"/api/v3/assets/{hash}", new ByteArrayContent(data.ToArray()), false, true);
        Assert.That(response?.Error, Is.Not.Null);
        Assert.That(response!.Error!.Name, Is.EqualTo(nameof(ApiModerationError)));
    }
}