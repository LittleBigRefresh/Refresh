using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using RefreshTests.GameServer.Extensions;
using Refresh.Database.Models.Authentication;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Playlists;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;
using Refresh.Database.Models.Playlists;
using Refresh.Database.Models.Levels;
using Refresh.Common.Constants;
using Refresh.Common.Extensions;
using Refresh.Database.Models.Assets;
using Refresh.Core.Configuration;
using Refresh.Database.Models;
using System.Net;

namespace RefreshTests.GameServer.Tests.ApiV3;

public class PlaylistApiTests : GameServerTest
{
    private const string TEST_IMAGE_HASH = "0ec63b140374ba704a58fa0c743cb357683313dd";

    [Test]
    public void CreateAndUpdatePlaylist()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);
        
        // Create
        ApiPlaylistCreationRequest request = new()
        {
            Name = "Hardest levels",
            Description = "idk"
        };
        ApiResponse<ApiGamePlaylistResponse>? response = client.PostData<ApiGamePlaylistResponse>($"/api/v3/playlists", request);
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.Name, Is.EqualTo(request.Name));
        Assert.That(response!.Data!.Description, Is.EqualTo(request.Description));
        context.Database.Refresh();

        // Ensure the playlist is in the root playlist
        GamePlaylist? root = context.Database.GetUserRootPlaylist(user);
        Assert.That(root, Is.Not.Null);
        Assert.That(context.Database.GetPlaylistsInPlaylist(root!, 0, 100).Items.Any(p => p.PlaylistId == response.Data.PlaylistId), Is.True);
        context.Database.Refresh();

        // Update
        ApiPlaylistCreationRequest update = new()
        {
            Description = "Read the description smh"
        };
        response = client.PatchData<ApiGamePlaylistResponse>($"/api/v3/playlists/id/{response.Data.PlaylistId}", update);
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.Name, Is.EqualTo(request.Name));
        Assert.That(response!.Data!.Description, Is.EqualTo(update.Description));
        Assert.That(response!.Data!.Description, Is.Not.EqualTo(request.Description));
    }

    [Test]
    public void TrimPlaylistNameAndDescription()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);
        
        // Create
        ApiPlaylistCreationRequest request = new()
        {
            Name = new string('S', 600),
            Description = new string('S', 600)
        };
        ApiResponse<ApiGamePlaylistResponse>? response = client.PostData<ApiGamePlaylistResponse>($"/api/v3/playlists", request);
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.Name.Length, Is.EqualTo(UgcLimits.TitleLimit));
        Assert.That(response!.Data!.Description.Length, Is.EqualTo(UgcLimits.DescriptionLimit));
        context.Database.Refresh();

        // Update
        ApiPlaylistCreationRequest update = new()
        {
            Name = new string('S', 600),
            Description = new string('S', 600)
        };
        response = client.PatchData<ApiGamePlaylistResponse>($"/api/v3/playlists/id/{response.Data.PlaylistId}", update);
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.Name.Length, Is.EqualTo(UgcLimits.TitleLimit));
        Assert.That(response!.Data!.Description.Length, Is.EqualTo(UgcLimits.DescriptionLimit));
    }

    [Test]
    public void ClampPlaylistLocation()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);
        
        // Create
        ApiPlaylistCreationRequest request = new()
        {
            Location = new(1234567, -420)
        };
        ApiResponse<ApiGamePlaylistResponse>? response = client.PostData<ApiGamePlaylistResponse>($"/api/v3/playlists", request);
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.Location.X, Is.EqualTo(ushort.MaxValue));
        Assert.That(response!.Data!.Location.Y, Is.Zero);
        context.Database.Refresh();

        // Update
        ApiPlaylistCreationRequest update = new()
        {
            Location = new(-420, 510)
        };
        response = client.PatchData<ApiGamePlaylistResponse>($"/api/v3/playlists/id/{response.Data.PlaylistId}", update);
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.Location.X, Is.Zero);
        Assert.That(response!.Data!.Location.Y, Is.EqualTo(update.Location.Y));
    }

    [Test]
    public void CreatePlaylistInPlaylist()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GamePlaylist parent = context.CreatePlaylist(user);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);
        
        // Create
        ApiPlaylistCreationRequest request = new()
        {
            Name = "Tier 1",
            Description = "Tier 1 hard levels"
        };
        ApiResponse<ApiGamePlaylistResponse>? response = client.PostData<ApiGamePlaylistResponse>($"/api/v3/playlists?parentId={parent.PlaylistId}", request);
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.Name, Is.EqualTo(request.Name));
        Assert.That(context.Database.GetTotalPlaylistsInPlaylist(parent), Is.EqualTo(1));
        // Root playlist should be completely ignored while handling request (unless there is a reason this behaviour should change in the future)
        Assert.That(context.Database.GetUserRootPlaylist(user), Is.Null);
    }

    [Test]
    public void CantCreatePlaylistInsideMissingPlaylist()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);
        
        // Create
        ApiPlaylistCreationRequest request = new()
        {
            Name = "Tier 2",
            Description = "Tier 2 hard levels"
        };
        ApiResponse<ApiGamePlaylistResponse>? response = client.PostData<ApiGamePlaylistResponse>($"/api/v3/playlists?parentId=67", request, false, true);
        Assert.That(response?.Error, Is.Not.Null);
        Assert.That(response!.Error!.StatusCode, Is.EqualTo(NotFound));
        Assert.That(context.Database.GetTotalPlaylistsByAuthor(user), Is.Zero);
    }

    [Test]
    public void CantCreatePlaylistInsideOthersPlaylist()
    {
        using TestContext context = this.GetServer();
        GameUser creator = context.CreateUser();
        GameUser moron = context.CreateUser();
        GamePlaylist parent = context.CreatePlaylist(creator);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, moron);
        
        // Create
        ApiPlaylistCreationRequest request = new()
        {
            Name = "Tier 0",
            Description = "My levels!!"
        };
        ApiResponse<ApiGamePlaylistResponse>? response = client.PostData<ApiGamePlaylistResponse>($"/api/v3/playlists?parentId={parent.PlaylistId}", request, false, true);
        Assert.That(response?.Error, Is.Not.Null);
        Assert.That(response!.Error!.StatusCode, Is.EqualTo(BadRequest));
        Assert.That(context.Database.GetTotalPlaylistsByAuthor(moron), Is.Zero);
        Assert.That(context.Database.GetTotalPlaylistsInPlaylist(parent), Is.Zero);
    }

    [Test]
    [TestCase("", true, false)]
    [TestCase("0", true, false)]
    [TestCase("lul", false, false)]
    [TestCase(TEST_IMAGE_HASH, true, true)]
    [TestCase("gg", false, false)]
    [TestCase("g67", false, false)]
    [TestCase("g1000035", true, false)]
    public void TestPlaylistIcons(string icon, bool success, bool isRemoteAsset)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);

        if (isRemoteAsset)
        {
            context.Database.AddAssetToDatabase(new()
            {
                AssetHash = icon,
                AssetType = GameAssetType.Png,
            });
        }
        
        // Create
        ApiPlaylistCreationRequest request = new()
        {
            Icon = icon
        };
        ApiResponse<ApiGamePlaylistResponse>? response = client.PostData<ApiGamePlaylistResponse>($"/api/v3/playlists", request, success, !success);
        Assert.That(response, Is.Not.Null);
        if (success)
        {
            Assert.That(response!.Data, Is.Not.Null);
            Assert.That(response.Data!.IconHash, Is.EqualTo(icon.IsBlankHash() ? "0" : icon));
        }
        
        GamePlaylist playlist = context.CreatePlaylist(user);
        response = client.PatchData<ApiGamePlaylistResponse>($"/api/v3/playlists/id/{playlist.PlaylistId}", request, success, !success);
        if (success)
        {
            Assert.That(response!.Data, Is.Not.Null);
            Assert.That(response.Data!.IconHash, Is.EqualTo(icon.IsBlankHash() ? "0" : icon));
        }
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

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);
        int publishAttempts = 0;

        // Not blocked yet
        Assert.That(context.Database.GetUploadRateLimit(user, GameDatabaseEntity.Playlist), Is.Null);
        Assert.That(context.Database.GetRemainingTimeIfUploadRateLimitReached(user, GameDatabaseEntity.Playlist, uploadConfig.UploadQuota), Is.Null);

        // Fill up half of quota
        publishAttempts += SpamUploadPlaylists(uploadConfig.UploadQuota / 2, client);
        context.Database.Refresh();

        // There is rate-limit data in DB, but the rate-limit hasn't been triggered yet
        EntityUploadRateLimit? uploadLimit = context.Database.GetUploadRateLimit(user, GameDatabaseEntity.Playlist);
        Assert.That(uploadLimit, Is.Not.Null);
        Assert.That(uploadLimit!.UploadCount, Is.EqualTo(uploadConfig.UploadQuota / 2));
        Assert.That(context.Database.GetRemainingTimeIfUploadRateLimitReached(user, GameDatabaseEntity.Playlist, uploadConfig.UploadQuota), Is.Null);
        
        // Try to upload more playlists
        publishAttempts += SpamUploadPlaylists(uploadConfig.UploadQuota / 2, client);
        context.Database.Refresh();

        // There is rate-limit data in DB, and the rate-limit has been triggered
        uploadLimit = context.Database.GetUploadRateLimit(user, GameDatabaseEntity.Playlist);
        Assert.That(uploadLimit, Is.Not.Null);
        Assert.That(uploadLimit!.UploadCount, Is.EqualTo(uploadConfig.UploadQuota));
        Assert.That(context.Database.GetRemainingTimeIfUploadRateLimitReached(user, GameDatabaseEntity.Playlist, uploadConfig.UploadQuota), Is.Not.Null);

        // Playlists are blocked
        publishAttempts += SpamUploadPlaylists(uploadAttemptsAfterExceeding, client, BadRequest);
        context.Database.Refresh();

        // Check amount of playlists
        Assert.That(context.Database.GetTotalPlaylistsByAuthor(user), Is.EqualTo(uploadConfig.UploadQuota));

        // Expire limit naturally by trying to publish again later
        context.Time.TimestampMilliseconds = 1000 * 60 * 60 * timeSpanHours + 10;
        publishAttempts += SpamUploadPlaylists(uploadAttemptsAfterExceeding, client);
        context.Database.Refresh();

        // there are more playlists now
        Assert.That(context.Database.GetTotalPlaylistsByAuthor(user), Is.EqualTo(uploadConfig.UploadQuota * 2));
    }

    private int SpamUploadPlaylists(int uploads, HttpClient client, HttpStatusCode expectedStatus = OK)
    {
        for (int i = 0; i < uploads; i++)
        {
            // Playlist requests currently don't need to reference unique assets
            ApiPlaylistCreationRequest playlist = new()
            {
                Name = "hi",
                Icon = "0",
                Description = "hi",
                Location = GameLocation.Random,
            };

            bool expectSuccess = expectedStatus == OK;
            ApiResponse<ApiGamePlaylistResponse>? response = client.PostData<ApiGamePlaylistResponse>($"/api/v3/playlists", playlist, expectSuccess, !expectSuccess);
            if (expectSuccess) Assert.That(response?.Data, Is.Not.Null);
            else               Assert.That(response?.Error, Is.Not.Null);
        }

        return uploads;
    }

    [Test]
    public async Task GetAndDeletePlaylist()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GamePlaylist playlist = context.CreatePlaylist(user);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);

        // Get (unauthenticated)
        ApiResponse<ApiGamePlaylistResponse>? response = context.Http.GetData<ApiGamePlaylistResponse>($"/api/v3/playlists/id/{playlist.PlaylistId}");
        Assert.That(response?.Data, Is.Not.Null);
        Assert.That(response!.Data!.PlaylistId, Is.EqualTo(playlist.PlaylistId));

        // Delete (authenticated)
        HttpResponseMessage deletionResponse = await client.DeleteAsync($"/api/v3/playlists/id/{playlist.PlaylistId}");
        Assert.That(deletionResponse, Is.Not.Null);
        Assert.That(deletionResponse.IsSuccessStatusCode, Is.True);

        GamePlaylist? postDeletionPlaylist = context.Database.GetPlaylistById(playlist.PlaylistId);
        Assert.That(postDeletionPlaylist, Is.Null);
    }

    [Test]
    public async Task AddAndRemoveLevelToPlaylist()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);
        GamePlaylist playlist = context.CreatePlaylist(user);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);

        // Add
        HttpResponseMessage response = await client.PostAsync($"/api/v3/playlists/id/{playlist.PlaylistId}/addLevel/id/{level.LevelId}", null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(context.Database.GetTotalLevelsInPlaylist(playlist), Is.EqualTo(1));
        Assert.That(context.Database.GetTotalPlaylistsContainingLevel(level), Is.EqualTo(1));

        // Remove
        response = await client.PostAsync($"/api/v3/playlists/id/{playlist.PlaylistId}/removeLevel/id/{level.LevelId}", null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(context.Database.GetTotalLevelsInPlaylist(playlist), Is.Zero);
        Assert.That(context.Database.GetTotalPlaylistsContainingLevel(level), Is.Zero);
    }

    [Test]
    public async Task CantAddAndRemoveLevelToOthersPlaylist()
    {
        using TestContext context = this.GetServer();
        GameUser creator = context.CreateUser();
        GameUser moron = context.CreateUser();
        GameLevel level = context.CreateLevel(moron);
        GamePlaylist playlist = context.CreatePlaylist(creator);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, moron);

        // Add (fail)
        HttpResponseMessage response = await client.PostAsync($"/api/v3/playlists/id/{playlist.PlaylistId}/addLevel/id/{level.LevelId}", null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(context.Database.GetTotalLevelsInPlaylist(playlist), Is.Zero);
        Assert.That(context.Database.GetTotalPlaylistsContainingLevel(level), Is.Zero);

        // Add (prepare for removal)
        context.Database.AddLevelToPlaylist(level, playlist);
        Assert.That(context.Database.GetTotalLevelsInPlaylist(playlist), Is.EqualTo(1));
        Assert.That(context.Database.GetTotalPlaylistsContainingLevel(level), Is.EqualTo(1));

        // Remove
        response = await client.PostAsync($"/api/v3/playlists/id/{playlist.PlaylistId}/removeLevel/id/{level.LevelId}", null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(context.Database.GetTotalLevelsInPlaylist(playlist), Is.EqualTo(1));
        Assert.That(context.Database.GetTotalPlaylistsContainingLevel(level), Is.EqualTo(1));
    }

    [Test]
    public async Task AddAndRemovePlaylistToPlaylist()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GamePlaylist parent = context.CreatePlaylist(user);
        GamePlaylist child = context.CreatePlaylist(user);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, user);

        // Add
        HttpResponseMessage response = await client.PostAsync($"/api/v3/playlists/id/{parent.PlaylistId}/addPlaylist/id/{child.PlaylistId}", null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(context.Database.GetTotalPlaylistsInPlaylist(parent), Is.EqualTo(1));
        Assert.That(context.Database.GetTotalPlaylistsContainingPlaylist(child), Is.EqualTo(1));

        // Remove
        response = await client.PostAsync($"/api/v3/playlists/id/{parent.PlaylistId}/removePlaylist/id/{child.PlaylistId}", null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(context.Database.GetTotalPlaylistsInPlaylist(parent), Is.Zero);
        Assert.That(context.Database.GetTotalPlaylistsContainingPlaylist(child), Is.Zero);
    }

    [Test]
    public async Task CantAddAndRemovePlaylistToOthersPlaylist()
    {
        using TestContext context = this.GetServer();
        GameUser creator = context.CreateUser();
        GameUser moron = context.CreateUser();
        GamePlaylist parent = context.CreatePlaylist(creator);
        GamePlaylist child = context.CreatePlaylist(moron);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Api, moron);

        // Add (fail)
        HttpResponseMessage response = await client.PostAsync($"/api/v3/playlists/id/{parent.PlaylistId}/addPlaylist/id/{child.PlaylistId}", null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(context.Database.GetTotalPlaylistsInPlaylist(parent), Is.Zero);
        Assert.That(context.Database.GetTotalPlaylistsContainingPlaylist(child), Is.Zero);

        // Add (prepare for removal)
        context.Database.AddPlaylistToPlaylist(child, parent);
        Assert.That(context.Database.GetTotalPlaylistsInPlaylist(parent), Is.EqualTo(1));
        Assert.That(context.Database.GetTotalPlaylistsContainingPlaylist(child), Is.EqualTo(1));

        // Remove
        response = await client.PostAsync($"/api/v3/playlists/id/{parent.PlaylistId}/removePlaylist/id/{child.PlaylistId}", null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(context.Database.GetTotalPlaylistsInPlaylist(parent), Is.EqualTo(1));
        Assert.That(context.Database.GetTotalPlaylistsContainingPlaylist(child), Is.EqualTo(1));
    }
}