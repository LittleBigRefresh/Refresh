using System.Net;
using Refresh.Common.Constants;
using Refresh.Core.Configuration;
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
        GamePlaylist root = SuccessfullyUploadRootPlaylistViaLBP1(client, user, context.Database);

        // Create actual playlist
        SerializedLbp1Playlist request = new()
        {
            Name = "real",
            Icon = ValidIconGuid,
            Description = "DESCRIPTION",
            Location = new GameLocation(),
        };

        HttpResponseMessage message = client.PostAsync($"/lbp/createPlaylist?parent_id={root.PlaylistId}", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedLbp1Playlist subResponse = message.Content.ReadAsXML<SerializedLbp1Playlist>();
        Assert.That(subResponse.Name, Is.EqualTo("real"));

        // Ensure the playlists are properly fetchable (root by itself is already asserted in its helper method)
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

    private GamePlaylist SuccessfullyUploadRootPlaylistViaLBP1(HttpClient client, GameUser user, GameDatabaseContext database)
    {
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

        GamePlaylist? root = database.GetUserRootPlaylist(user);
        Assert.That(root, Is.Not.Null);
        Assert.That(root!.IsRoot, Is.True);
        Assert.That(root!.PlaylistId, Is.EqualTo(rootResponse.Id));
        Assert.That(root!.Name, Is.EqualTo("root"));

        return root;
    }

    [Test]
    public void CreateSubPlaylist()
    {
        using TestContext context = this.GetServer();

        GameUser user = context.CreateUser();
        GamePlaylist root = context.Database.CreatePlaylist(user, new SerializedLbp1Playlist()
        {
            Name = "root",
            Icon = ValidIconGuid,
            Description = "d",
            Location = GameLocation.Zero
        }, true);
        Assert.That(context.Database.GetUserRootPlaylist(user), Is.Not.Null);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, user);
        
        // Create sub-playlist of root
        SerializedLbp1Playlist request = new()
        {
            Name = "hi",
            Icon = ValidIconGuid,
            Description = "DESCRIPTION",
            Location = new GameLocation(),
        };

        HttpResponseMessage message = client.PostAsync($"/lbp/createPlaylist?parent_id={root.PlaylistId}", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        Assert.That(context.Database.GetTotalPlaylistsInPlaylist(root), Is.EqualTo(1));
        Assert.That(context.Database.GetTotalPlaylistsByAuthor(user), Is.EqualTo(1)); // does not include root
    }

    [Test]
    public void CannotCreateSubPlaylistWhileReadOnlyMode()
    {
        using TestContext context = this.GetServer();
        GameServerConfig config = context.Server.Value.GameServerConfig;
        config.NormalUserPermissions.ReadOnlyMode = true;
        config.TrustedUserPermissions.ReadOnlyMode = true;

        GameUser user = context.CreateUser();
        GamePlaylist root = context.Database.CreatePlaylist(user, new SerializedLbp1Playlist()
        {
            Name = "root",
            Icon = ValidIconGuid,
            Description = "d",
            Location = GameLocation.Zero
        }, true);
        Assert.That(context.Database.GetUserRootPlaylist(user), Is.Not.Null);
        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, user);
        
        // Create sub-playlist of root
        SerializedLbp1Playlist request = new()
        {
            Name = "hi",
            Icon = ValidIconGuid,
            Description = "DESCRIPTION",
            Location = new GameLocation(),
        };

        HttpResponseMessage message = client.PostAsync($"/lbp/createPlaylist?parent_id={root.PlaylistId}", new StringContent(request.AsXML())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(Unauthorized));
        Assert.That(context.Database.GetTotalPlaylistsByAuthor(user), Is.Zero); // does not include root
    }

    [Test]
    public void CanCreateRootPlaylistWhileReadOnlyMode()
    {
        using TestContext context = this.GetServer();
        GameServerConfig config = context.Server.Value.GameServerConfig;
        config.NormalUserPermissions.ReadOnlyMode = true;
        config.TrustedUserPermissions.ReadOnlyMode = true;

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
        Assert.That(context.Database.GetUserRootPlaylist(user), Is.Not.Null);
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

    [Test]
    [TestCase(4, 4, 1)]
    public void PlaylistUploadsGetRateLimitedTemporarily(int playlistQuota, int uploadAttemptsAfterExceeding, int timeSpanHours)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser("PlaylistOTrolle");

        // Prepare config
        GameServerConfig config = context.Server.Value.GameServerConfig;
        EntityUploadRateLimitProperties uploadConfig = new()
        {
            Enabled = true,
            UploadQuota = playlistQuota,
            TimeSpanHours = timeSpanHours,
        };
        config.NormalUserPermissions.PlaylistUploadRateLimit = uploadConfig;

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        int publishAttempts = 0;

        // create root via LBP1 first; since root doesn't count towards the rate-limit, we can, this way, both ensure that the rate-limit properly
        // starts tracking the spam uploads below, and we can properly ensure that roots do, infact, not count towards the rate-limit here
        SuccessfullyUploadRootPlaylistViaLBP1(client, user, context.Database);

        // Not blocked yet
        Assert.That(context.Database.GetUploadRateLimit(user, GameDatabaseEntity.Playlist), Is.Null);
        Assert.That(context.Database.GetRemainingTimeIfUploadRateLimitReached(user, GameDatabaseEntity.Playlist, uploadConfig.UploadQuota), Is.Null);

        // Fill up half of quota in lbp1
        publishAttempts += SpamUploadPlaylistsInLBP1(uploadConfig.UploadQuota / 2, client);
        context.Database.Refresh();

        // There is rate-limit data in DB, but the rate-limit hasn't been triggered yet
        EntityUploadRateLimit? uploadLimit = context.Database.GetUploadRateLimit(user, GameDatabaseEntity.Playlist);
        Assert.That(uploadLimit, Is.Not.Null);
        Assert.That(uploadLimit!.UploadCount, Is.EqualTo(uploadConfig.UploadQuota / 2));
        Assert.That(context.Database.GetRemainingTimeIfUploadRateLimitReached(user, GameDatabaseEntity.Playlist, uploadConfig.UploadQuota), Is.Null);
        
        // Try to upload more playlists (in lbp3 now) to reach quota, this way we also ensure that both endpoints share the same rate-limit
        publishAttempts += SpamUploadPlaylistsInLBP3(uploadConfig.UploadQuota / 2, client);
        context.Database.Refresh();

        // Ensure there were no notifications sent
        Assert.That(context.Database.GetNotificationCountByUser(user), Is.Zero);

        // There is rate-limit data in DB, and the rate-limit has been triggered
        uploadLimit = context.Database.GetUploadRateLimit(user, GameDatabaseEntity.Playlist);
        Assert.That(uploadLimit, Is.Not.Null);
        Assert.That(uploadLimit!.UploadCount, Is.EqualTo(uploadConfig.UploadQuota));
        Assert.That(context.Database.GetRemainingTimeIfUploadRateLimitReached(user, GameDatabaseEntity.Playlist, uploadConfig.UploadQuota), Is.Not.Null);

        // Playlists are blocked
        publishAttempts += SpamUploadPlaylistsInLBP1(uploadAttemptsAfterExceeding, client, Unauthorized);
        context.Database.Refresh();

        // Check amount of playlists
        Assert.That(context.Database.GetTotalPlaylistsByAuthor(user), Is.EqualTo(uploadConfig.UploadQuota));

        // Expire limit naturally by trying to publish again later
        context.Time.TimestampMilliseconds = 1000 * 60 * 60 * timeSpanHours + 10;
        publishAttempts += SpamUploadPlaylistsInLBP3(uploadAttemptsAfterExceeding, client);
        context.Database.Refresh();

        // there are more playlists now
        Assert.That(context.Database.GetTotalPlaylistsByAuthor(user), Is.EqualTo(uploadConfig.UploadQuota * 2));
    }

    private int SpamUploadPlaylistsInLBP1(int uploads, HttpClient client, HttpStatusCode expectedStatus = OK)
    {
        for (int i = 0; i < uploads; i++)
        {
            // Playlist requests currently don't need to reference unique assets
            SerializedLbp1Playlist playlist = new()
            {
                Name = "hi",
                Icon = "0",
                Description = "i'm not gonna stop spamming playlists",
                Location = GameLocation.Random,
            };

            HttpResponseMessage message = client.PostAsync($"/lbp/createPlaylist", new StringContent(playlist.AsXML())).Result;
            Assert.That(message.StatusCode, Is.EqualTo(expectedStatus));
        }

        return uploads;
    }

    private int SpamUploadPlaylistsInLBP3(int uploads, HttpClient client, HttpStatusCode expectedStatus = OK)
    {
        for (int i = 0; i < uploads; i++)
        {
            // Playlist requests currently don't need to reference unique assets
            SerializedLbp3Playlist playlist = new()
            {
                Name = "hi",
                Description = "i didn't stop spamming playlists",
            };

            HttpResponseMessage message = client.PostAsync($"/lbp/playlists", new StringContent(playlist.AsXML())).Result;
            Assert.That(message.StatusCode, Is.EqualTo(expectedStatus));
        }

        return uploads;
    }
}