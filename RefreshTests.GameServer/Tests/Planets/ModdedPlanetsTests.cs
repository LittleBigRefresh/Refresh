using Refresh.Core.Configuration;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Endpoints.DataTypes.Response;
using Refresh.Interfaces.Game.Types.UserData;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Planets;

public class ModdedPlanetsTests : GameServerTest
{
    private const string TEST_LEVEL_HASH = "acddf3f9251c1ddb675ad81ba34ba16135b54aca";
    private const string TEST_LEVEL_HASH_2 = "59b250969f6b0b05ea352e4b7efa597efa0f7d21";
    private const string TEST_VOIP_HASH = "148c07876f15ef9ab90cc93e4900daa003214ae7";
    private const string TEST_TEXTURE_HASH = "9488801db61c5313db3bb15db7d66fd26df7e789";
    private const string TEST_MESH_HASH = "ca238f89938fe835049eaa72e79157dc9e292ad8";

    private void UploadUnmoddedPlanet(TestContext context, HttpClient client, string rootAssetHash)
    {
        // Upload planet dependency and make the planet level depend on the texture
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{TEST_TEXTURE_HASH}", new ReadOnlyMemoryContent("TEX "u8.ToArray())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        context.Database.AddOrOverwriteAssetDependencyRelations(rootAssetHash, [TEST_TEXTURE_HASH]);

        // Update user in-game
        SerializedUpdateDataProfile request = new()
        {
            PlanetsHash = rootAssetHash,
        };

        message = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
    }

    private void UploadLightlyModdedPlanet(TestContext context, HttpClient client, string rootAssetHash)
    {
        // Upload planet dependency and make the planet level depend on the voice recording
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{TEST_VOIP_HASH}", new ReadOnlyMemoryContent("VOPb"u8.ToArray())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        context.Database.AddOrOverwriteAssetDependencyRelations(rootAssetHash, [TEST_VOIP_HASH]);

        // Update user in-game
        SerializedUpdateDataProfile request = new()
        {
            PlanetsHash = rootAssetHash,
        };

        message = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
    }

    private void UploadHeavilyModdedPlanet(TestContext context, HttpClient client, string rootAssetHash)
    {
        // Prepare config so normal users may upload modded assets
        GameServerConfig config = context.Server.Value.GameServerConfig;
        config.BlockedAssetFlags = new(AssetFlags.Dangerous);

        // Upload planet dependency and make the planet level depend on the voice recording
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{TEST_MESH_HASH}", new ReadOnlyMemoryContent("MSHb"u8.ToArray())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        context.Database.AddOrOverwriteAssetDependencyRelations(rootAssetHash, [TEST_MESH_HASH]);

        context.Database.Refresh();

        // Update user in-game
        SerializedUpdateDataProfile request = new()
        {
            PlanetsHash = rootAssetHash,
        };

        message = client.PostAsync($"/lbp/updateUser", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ShowUnmoddedPlanets(bool showModdedPlanets)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        context.Database.SetShowModdedPlanets(user, showModdedPlanets);
        using HttpClient client1 = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet2, TokenPlatform.PS3, out string _, user);

        GameUser publisher = context.CreateUser();
        using HttpClient client2 = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet2, TokenPlatform.PS3, out string _, publisher);

        // Upload root planet asset
        HttpResponseMessage message = client2.PostAsync($"/lbp/upload/{TEST_LEVEL_HASH}", new ReadOnlyMemoryContent("LVLb"u8.ToArray())).Result;
        this.UploadUnmoddedPlanet(context, client2, TEST_LEVEL_HASH);
        context.Database.Refresh();

        // Ensure the planet has been correctly flagged as unmodded
        Assert.That(context.Database.GetUserByObjectId(publisher.UserId)!.AreLbp2PlanetsModded, Is.False);

        // Now get the publisher's user data as the other user
        message = client1.GetAsync($"/lbp/user/{publisher.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        GameUserResponse response = message.Content.ReadAsXML<GameUserResponse>();
        Assert.That(response.PlanetsHash, Is.EqualTo(TEST_LEVEL_HASH));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ShowOrHideLightlyModdedPlanets(bool showModdedPlanets)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        context.Database.SetShowModdedPlanets(user, showModdedPlanets);
        using HttpClient client1 = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet2, TokenPlatform.PS3, out string _, user);

        GameUser publisher = context.CreateUser();
        using HttpClient client2 = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet2, TokenPlatform.PS3, out string _, publisher);

        // Upload root planet asset
        HttpResponseMessage message = client2.PostAsync($"/lbp/upload/{TEST_LEVEL_HASH}", new ReadOnlyMemoryContent("LVLb"u8.ToArray())).Result;
        this.UploadLightlyModdedPlanet(context, client2, TEST_LEVEL_HASH);
        context.Database.Refresh();

        // Ensure the planet has been correctly flagged as modded
        Assert.That(context.Database.GetUserByObjectId(publisher.UserId)!.AreLbp2PlanetsModded, Is.True);

        // Now get the publisher's user data as the other user
        message = client1.GetAsync($"/lbp/user/{publisher.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        GameUserResponse response = message.Content.ReadAsXML<GameUserResponse>();
        Assert.That(response.PlanetsHash, Is.EqualTo(showModdedPlanets ? TEST_LEVEL_HASH : "0"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ShowOrHideHeavilyModdedPlanets(bool showModdedPlanets)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        context.Database.SetShowModdedPlanets(user, showModdedPlanets);
        using HttpClient client1 = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet2, TokenPlatform.PS3, out string _, user);

        GameUser publisher = context.CreateUser();
        using HttpClient client2 = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet2, TokenPlatform.PS3, out string _, publisher);

        // Upload root planet asset
        HttpResponseMessage message = client2.PostAsync($"/lbp/upload/{TEST_LEVEL_HASH}", new ReadOnlyMemoryContent("LVLb"u8.ToArray())).Result;
        this.UploadHeavilyModdedPlanet(context, client2, TEST_LEVEL_HASH);
        context.Database.Refresh();

        // Ensure the planet has been correctly flagged as modded
        Assert.That(context.Database.GetUserByObjectId(publisher.UserId)!.AreLbp2PlanetsModded, Is.True);

        // Now get the publisher's user data as the other user
        message = client1.GetAsync($"/lbp/user/{publisher.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        GameUserResponse response = message.Content.ReadAsXML<GameUserResponse>();
        Assert.That(response.PlanetsHash, Is.EqualTo(showModdedPlanets ? TEST_LEVEL_HASH : "0"));
    }

    [Test]
    public void ModdedLbp2PlanetDoesntHideUnmoddedVitaPlanet()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        context.Database.SetShowModdedPlanets(user, false);
        using HttpClient client1Lbp2 = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet2, TokenPlatform.PS3, out string _, user);
        using HttpClient client1Vita = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanetVita, TokenPlatform.Vita, out string _, user);

        // Prepare config so normal users may upload modded assets
        GameServerConfig config = context.Server.Value.GameServerConfig;
        config.BlockedAssetFlags = new(AssetFlags.Dangerous);

        GameUser publisher = context.CreateUser();
        using HttpClient client2LBP2 = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet2, TokenPlatform.PS3, out string _, publisher);
        using HttpClient client2Vita = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanetVita, TokenPlatform.Vita, out string _, publisher);

        // Upload root planet assets
        HttpResponseMessage message = client2Vita.PostAsync($"/lbp/upload/{TEST_LEVEL_HASH}", new ReadOnlyMemoryContent("LVLb"u8.ToArray())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        message = client2LBP2.PostAsync($"/lbp/upload/{TEST_LEVEL_HASH_2}", new ReadOnlyMemoryContent("LVLblol"u8.ToArray())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        this.UploadUnmoddedPlanet(context, client2Vita, TEST_LEVEL_HASH);
        this.UploadLightlyModdedPlanet(context, client2LBP2, TEST_LEVEL_HASH_2);
        context.Database.Refresh();

        // Ensure the LBP2 planet has been correctly flagged as modded, and Vita as unmodded
        GameUser updated = context.Database.GetUserByObjectId(publisher.UserId)!;
        Assert.That(updated.AreLbp2PlanetsModded, Is.True);
        Assert.That(updated.AreVitaPlanetsModded, Is.False);

        // Now get the publisher's user data from various games
        message = client1Lbp2.GetAsync($"/lbp/user/{publisher.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        GameUserResponse response = message.Content.ReadAsXML<GameUserResponse>();
        Assert.That(response.PlanetsHash, Is.EqualTo("0"));

        message = client1Vita.GetAsync($"/lbp/user/{publisher.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        response = message.Content.ReadAsXML<GameUserResponse>();
        Assert.That(response.PlanetsHash, Is.EqualTo(TEST_LEVEL_HASH));
    }

    [Test]
    public void UpdateShowModdedPlanetsIngame()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        context.Database.VerifyUserEmail(user);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet2, TokenPlatform.PS3, out string _, user);

        // Disable and ensure the option is set to hide
        HttpResponseMessage message = client.PostAsync("/lbp/filter", new StringContent("/hidemodp")).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        context.Database.Refresh();
        Assert.That(context.Database.GetUserByObjectId(user.UserId)!.ShowModdedPlanets, Is.False);

        // Enable again and ensure the option is set to show
        message = client.PostAsync("/lbp/filter", new StringContent("/showmodp")).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        context.Database.Refresh();
        Assert.That(context.Database.GetUserByObjectId(user.UserId)!.ShowModdedPlanets, Is.True);
    }

    [Test]
    public void DontHideOwnModdedPlanets()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        context.Database.SetShowModdedPlanets(user, false);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet2, TokenPlatform.PS3, out string _, user);

        // Upload root planet asset
        HttpResponseMessage message = client.PostAsync($"/lbp/upload/{TEST_LEVEL_HASH}", new ReadOnlyMemoryContent("LVLb"u8.ToArray())).Result;
        this.UploadHeavilyModdedPlanet(context, client, TEST_LEVEL_HASH);
        context.Database.Refresh();

        // Ensure the planet has been correctly flagged as modded
        Assert.That(context.Database.GetUserByObjectId(user.UserId)!.AreLbp2PlanetsModded, Is.True);

        // Now get the publisher's user data as the other user
        message = client.GetAsync($"/lbp/user/{user.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Ensure the planet hash attribute is set to the modded planet independently of the user's preference
        GameUserResponse response = message.Content.ReadAsXML<GameUserResponse>();
        Assert.That(response.PlanetsHash, Is.EqualTo(TEST_LEVEL_HASH));
    }
}