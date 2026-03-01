using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using RefreshTests.GameServer.Extensions;
using Refresh.Database.Models.Authentication;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Playlists;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Request;
using Refresh.Database.Models.Playlists;
using Refresh.Database.Models.Levels;
using Refresh.Common.Constants;

namespace RefreshTests.GameServer.Tests.ApiV3;

public class PlaylistApiTests : GameServerTest
{
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