using Refresh.Common.Exceptions;
using Refresh.Database;
using Refresh.Database.Extensions;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Playlists;
using Refresh.Database.Models.Users;

namespace RefreshTests.GameServer.Tests.Playlists;

public class PlaylistAdditionTests : GameServerTest
{
    [Test]
    public void CanAddPlaylistToAnotherPlaylist()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GamePlaylist parent = context.CreatePlaylist(user);
        GamePlaylist child = context.CreatePlaylist(user);

        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, user);

        HttpResponseMessage message = client.PostAsync($"/lbp/addToPlaylist/{parent.PlaylistId}?slot_type=playlist&slot_id={child.PlaylistId}",
            new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        // Assert that child was actually added to parent
        DatabaseList<GamePlaylist> childPlaylists = context.Database.GetPlaylistsInPlaylist(parent, 0, 10);
        Assert.That(childPlaylists.TotalItems, Is.EqualTo(1));
        Assert.That(childPlaylists.Items.Count(), Is.EqualTo(1));
        Assert.That(childPlaylists.Items.First().PlaylistId, Is.EqualTo(child.PlaylistId));

        DatabaseList<GamePlaylist> parentPlaylists = context.Database.GetPlaylistsContainingPlaylist(child, 0, 10);
        Assert.That(parentPlaylists.TotalItems, Is.EqualTo(1));
        Assert.That(parentPlaylists.Items.Count(), Is.EqualTo(1));
        Assert.That(parentPlaylists.Items.First().PlaylistId, Is.EqualTo(parent.PlaylistId));

        // Trying to add again shouldn't cause any errors
        message = client.PostAsync($"/lbp/addToPlaylist/{parent.PlaylistId}?slot_type=playlist&slot_id={child.PlaylistId}",
            new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
    }

    [Test]
    public void CannotAddPlaylistToItself()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GamePlaylist playlist = context.CreatePlaylist(user);

        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, user);

        HttpResponseMessage message = client.PostAsync($"/lbp/addToPlaylist/{playlist.PlaylistId}?slot_type=playlist&slot_id={playlist.PlaylistId}",
            new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));

        // Assert that child was actually added to parent
        DatabaseList<GamePlaylist> childPlaylists = context.Database.GetPlaylistsInPlaylist(playlist, 0, 10);
        Assert.That(childPlaylists.TotalItems, Is.Zero);
        Assert.That(childPlaylists.Items.Count(), Is.Zero);

        DatabaseList<GamePlaylist> parentPlaylists = context.Database.GetPlaylistsContainingPlaylist(playlist, 0, 10);
        Assert.That(parentPlaylists.TotalItems, Is.Zero);
        Assert.That(parentPlaylists.Items.Count(), Is.Zero);
    }

    [Test]
    public void CannotAddPlaylistToOwnChild() // else infinite loop
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GamePlaylist parent = context.CreatePlaylist(user);
        GamePlaylist child = context.CreatePlaylist(user);
        context.Database.AddPlaylistToPlaylist(child, parent);

        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, user);

        HttpResponseMessage message = client.PostAsync($"/lbp/addToPlaylist/{child.PlaylistId}?slot_type=playlist&slot_id={parent.PlaylistId}",
            new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));

        // Assert that parent contains child, but child doesn't contain parent
        DatabaseList<GamePlaylist> childPlaylistsOfParent = context.Database.GetPlaylistsInPlaylist(parent, 0, 10);
        Assert.That(childPlaylistsOfParent.TotalItems, Is.EqualTo(1));
        Assert.That(childPlaylistsOfParent.Items.Count(), Is.EqualTo(1));
        Assert.That(childPlaylistsOfParent.Items.First().PlaylistId, Is.EqualTo(child.PlaylistId));

        DatabaseList<GamePlaylist> childPlaylistsOfChild = context.Database.GetPlaylistsInPlaylist(child, 0, 10);
        Assert.That(childPlaylistsOfChild.TotalItems, Is.Zero);
        Assert.That(childPlaylistsOfChild.Items.Count(), Is.Zero);

        // Assert child is in parent, but parent isn't in child
        DatabaseList<GamePlaylist> parentPlaylistsOfChild = context.Database.GetPlaylistsContainingPlaylist(child, 0, 10);
        Assert.That(parentPlaylistsOfChild.TotalItems, Is.EqualTo(1));
        Assert.That(parentPlaylistsOfChild.Items.Count(), Is.EqualTo(1));
        Assert.That(parentPlaylistsOfChild.Items.First().PlaylistId, Is.EqualTo(parent.PlaylistId));

        DatabaseList<GamePlaylist> parentPlaylistsOfParent = context.Database.GetPlaylistsContainingPlaylist(parent, 0, 10);
        Assert.That(parentPlaylistsOfParent.TotalItems, Is.Zero);
        Assert.That(parentPlaylistsOfParent.Items.Count(), Is.Zero);
    }

    [Test]
    public void LoopDetectorStopsWhenMaxTraversalReached()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        GamePlaylist root = context.CreatePlaylist(user);
        GamePlaylist inbetween = context.CreatePlaylist(user);
        GamePlaylist leaf = context.CreatePlaylist(user);
        GamePlaylist newRoot = context.CreatePlaylist(user);

        context.Database.AddPlaylistToPlaylist(inbetween, root);
        context.Database.AddPlaylistToPlaylist(leaf, inbetween);

        // There are 3 playlists in root's tree in total: root itself, inbetween, and leaf, so 2 and below should trigger the exception,
        // but 3 and above should be just enough for the method to not throw.
        Assert.That(() => newRoot.WillAddingChildCauseInfiniteLoop(root, context.Database, context.GetDataContext().Logger, 2),
            Throws.TypeOf<TakingTooLongException>());
        
        // Higher max count should allow the method to succeed normally
        Assert.That(() => newRoot.WillAddingChildCauseInfiniteLoop(root, context.Database, context.GetDataContext().Logger, 3),
            Throws.Nothing);
    }
}