using Refresh.Common.Constants;
using Refresh.Database;
using Refresh.Database.Models;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Playlists;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Types.Playlists;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Playlists;

public class PlaylistUploadTests : GameServerTest
{
    private const string ValidIconGuid = "g18451"; // star sticker
    private const string InvalidIconGuid = "g1087"; // sackboy model
    private const string IconHash = "9488801db61c5313db3bb15db7d66fd26df7e789"; // hash of "TEX "

    [Test]
    public void CreateAndUpdateLbp1Playlist()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, user);
        
        // Create root playlist
        SerializedLbp1Playlist request = new()
        {
            Name = "root",
            Icon = ValidIconGuid,
            Description = "DESCRIPTION",
            Location = new GameLocation(),
        };

        HttpResponseMessage message = client.PostAsync("/lbp/createPlaylist", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedLbp1Playlist rootResponse = message.Content.ReadAsXML<SerializedLbp1Playlist>();
        Assert.That(rootResponse.Name, Is.EqualTo("root"));

        // Create actual playlist
        request = new()
        {
            Name = "real",
            Icon = ValidIconGuid,
            Description = "DESCRIPTION",
            Location = new GameLocation(),
        };

        message = client.PostAsync($"/lbp/createPlaylist?parent_id={rootResponse.Id}", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedLbp1Playlist subResponse = message.Content.ReadAsXML<SerializedLbp1Playlist>();
        Assert.That(subResponse.Name, Is.EqualTo("real"));

        // Ensure the playlists are properly fetchable
        GamePlaylist? root = context.Database.GetUserRootPlaylist(user);
        Assert.That(root, Is.Not.Null);
        Assert.That(root!.PlaylistId, Is.EqualTo(rootResponse.Id));
        Assert.That(root!.PublisherId, Is.EqualTo(user.UserId));

        DatabaseList<GamePlaylist> playlists = context.Database.GetPlaylistsByAuthor(user, 0, 10);
        Assert.That(playlists.Items.Count, Is.EqualTo(1));

        playlists = context.Database.GetPlaylistsInPlaylist(root, 0, 10);
        Assert.That(playlists.Items.Count, Is.EqualTo(1));

        // Now update
        request = new()
        {
            Name = "legit",
            Icon = ValidIconGuid,
            Description = "DESCRIPTION",
            Location = new GameLocation(),
        };

        message = client.PostAsync($"/lbp/setPlaylistMetaData/{subResponse.Id}", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        context.Database.Refresh();

        GamePlaylist? updated = context.Database.GetPlaylistById(subResponse.Id);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Name, Is.EqualTo("legit"));
    }

    [Test]
    public void CreateAndUpdateLbp3Playlist()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);
        
        // Create playlist
        SerializedLbp3Playlist request = new()
        {
            Name = "real",
        };

        HttpResponseMessage message = client.PostAsync($"/lbp/playlists", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedLbp3Playlist response = message.Content.ReadAsXML<SerializedLbp3Playlist>();
        Assert.That(response.Name, Is.EqualTo("real"));
        Assert.That(response.Description, Is.EqualTo(""));

        // Ensure a root playlist was automatically created, and the actual playlist is in that root playlist
        GamePlaylist? root = context.Database.GetUserRootPlaylist(user);
        Assert.That(root, Is.Not.Null);
        Assert.That(root!.PlaylistId, Is.Not.EqualTo(response.Id));
        Assert.That(root!.PublisherId, Is.EqualTo(user.UserId));

        DatabaseList<GamePlaylist> playlists = context.Database.GetPlaylistsByAuthor(user, 0, 10);
        Assert.That(playlists.Items.Count, Is.EqualTo(1));
        
        playlists = context.Database.GetPlaylistsInPlaylist(root, 0, 10);
        Assert.That(playlists.Items.Count, Is.EqualTo(1));

        // Now update
        request = new()
        {
            Description = "DESCRIPTION",
        };

        message = client.PostAsync($"/lbp/playlists/{response.Id}", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        response = message.Content.ReadAsXML<SerializedLbp3Playlist>();
        Assert.That(response.Name, Is.EqualTo("real"));
        Assert.That(response.Description, Is.EqualTo("DESCRIPTION"));
    }

    [Test]
    public void TrimLbp1PlaylistStrings()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, user);
        
        // Create root playlist
        SerializedLbp1Playlist request = new()
        {
            Name = new string('*', UgcLimits.TitleLimit * 2),
            Icon = ValidIconGuid,
            Description = new string('*', UgcLimits.DescriptionLimit * 2),
            Location = new GameLocation(),
        };

        HttpResponseMessage message = client.PostAsync("/lbp/createPlaylist", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedLbp1Playlist response = message.Content.ReadAsXML<SerializedLbp1Playlist>();
        Assert.That(response.Name.Length, Is.EqualTo(UgcLimits.TitleLimit));
        Assert.That(response.Description.Length, Is.EqualTo(UgcLimits.DescriptionLimit));

        // Now update
        request = new()
        {
            Name = new string('d', UgcLimits.TitleLimit * 2),
            Icon = ValidIconGuid,
            Description = new string('d', UgcLimits.DescriptionLimit * 2),
            Location = new GameLocation(),
        };
        message = client.PostAsync($"/lbp/setPlaylistMetaData/{response.Id}", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        GamePlaylist? updated = context.Database.GetPlaylistById(response.Id);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Name.Length, Is.EqualTo(UgcLimits.TitleLimit));
        Assert.That(updated!.Description.Length, Is.EqualTo(UgcLimits.DescriptionLimit));
    }

    [Test]
    public void TrimLbp3PlaylistStrings()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, user);
        
        // Create root playlist
        SerializedLbp3Playlist request = new()
        {
            Name = new string('*', UgcLimits.TitleLimit * 2),
            Description = new string('*', UgcLimits.DescriptionLimit * 2),
        };

        HttpResponseMessage message = client.PostAsync("/lbp/playlists", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedLbp3Playlist response = message.Content.ReadAsXML<SerializedLbp3Playlist>();
        Assert.That(response.Name, Is.Not.Null);
        Assert.That(response.Description, Is.Not.Null);
        Assert.That(response.Name!.Length, Is.EqualTo(UgcLimits.TitleLimit));
        Assert.That(response.Description!.Length, Is.EqualTo(UgcLimits.DescriptionLimit));

        // Now update
        request = new()
        {
            Name = new string('d', UgcLimits.TitleLimit * 2),
            Description = new string('d', UgcLimits.DescriptionLimit * 2),
        };
        message = client.PostAsync($"/lbp/playlists/{response.Id}", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        context.Database.Refresh();

        response = message.Content.ReadAsXML<SerializedLbp3Playlist>();
        Assert.That(response.Name, Is.Not.Null);
        Assert.That(response.Description, Is.Not.Null);
        Assert.That(response.Name!.Length, Is.EqualTo(UgcLimits.TitleLimit));
        Assert.That(response.Description!.Length, Is.EqualTo(UgcLimits.DescriptionLimit));
    }

    [Test]
    [TestCase("", false, false)]
    [TestCase("0", true, false)]
    [TestCase(ValidIconGuid, true, false)]
    [TestCase(InvalidIconGuid, false, false)]
    [TestCase("hi", false, false)]
    [TestCase(IconHash, false, false)]
    [TestCase(IconHash, true, true)]
    public void ValidatePlaylistIcon(string iconHash, bool isValid, bool uploadAsset)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, user);

        // If this is a server asset, upload it
        if (uploadAsset)
        {
            HttpResponseMessage assetMessage = client.PostAsync($"/lbp/upload/{iconHash}", new ReadOnlyMemoryContent("TEX "u8.ToArray())).Result;
            Assert.That(assetMessage.StatusCode, Is.EqualTo(OK));
        }
        
        // Create playlist
        SerializedLbp1Playlist request = new()
        {
            Name = "",
            Icon = iconHash,
            Description = "",
            Location = new GameLocation(),
        };

        HttpResponseMessage message = client.PostAsync("/lbp/createPlaylist", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedLbp1Playlist response = message.Content.ReadAsXML<SerializedLbp1Playlist>();
        Assert.That(response.Icon, Is.EqualTo(isValid ? iconHash : "0"));

        // Now update
        message = client.PostAsync($"/lbp/setPlaylistMetaData/{response.Id}", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        GamePlaylist? updated = context.Database.GetPlaylistById(response.Id);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.IconHash, Is.EqualTo(isValid ? iconHash : "0"));
    }
}