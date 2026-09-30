using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Playlists;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Types.Lists;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Categories;

public class ByUserCategoryTests : GameServerTest
{
    private void SpamCreateEntities(TestContext context, int levelCount, int userCount)
    {
        GameUser publisher = context.CreateUser("MIKE");
        GamePlaylist roots = context.Database.CreateRootPlaylist(publisher);
        
        for (int i = 0; i < levelCount; i++)
        {
            context.CreateLevel(publisher, "some level " + i);
        }
        for (int i = 0; i < userCount; i++)
        {
            GamePlaylist playlist = context.CreatePlaylist(publisher, "some playlist " + i);
            context.Database.AddPlaylistToPlaylist(playlist, roots);
        }
        
        // Create one more non-root playlist but don't add it to root, to test that this category will only return
        // sub-playlists of root, not all playlists by this user.
        context.CreatePlaylist(publisher, "some other playlist");
    }
    
    [Test]
    public void InsertingPlaylistsIntoLBP1ResponseDoesntBreakPagination()
    {
        using TestContext context = this.GetServer();
        GameUser player = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, player);
        this.SpamCreateEntities(context, 35, 32);
        
        // Page 1
        HttpResponseMessage message = client.GetAsync($"/lbp/slots/by?u=MIKE&pageSize=30&pageStart=1").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedMinimalLevelList result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(60));
        Assert.That(result.Items.Count(s => s.Type == "user"), Is.EqualTo(30));
        Assert.That(result.Items.Count(s => s.Type == "playlist"), Is.EqualTo(30));
        Assert.That(result.Users, Has.Count.Zero);
        Assert.That(result.Total, Is.EqualTo(67)); // all levels + playlists
        Assert.That(result.NextPageStart, Is.EqualTo(31));
        
        // Page 2
        message = client.GetAsync($"/lbp/slots/by?u=MIKE&pageSize=30&pageStart=31").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(7));
        Assert.That(result.Items.Count(s => s.Type == "user"), Is.EqualTo(5));
        Assert.That(result.Items.Count(s => s.Type == "playlist"), Is.EqualTo(2));
        Assert.That(result.Users, Has.Count.Zero);
        Assert.That(result.Total, Is.EqualTo(67)); // still the same
        Assert.That(result.NextPageStart, Is.Zero);
    }
    
    [Test]
    public void NonLBP1DoesntReceivePlaylists()
    {
        using TestContext context = this.GetServer();
        GameUser player = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet2, TokenPlatform.PS3, player);
        this.SpamCreateEntities(context, 35, 32);
        
        // No need to test pagination, but do still test the received pagination data
        HttpResponseMessage message = client.GetAsync($"/lbp/slots/by?u=MIKE&pageSize=30&pageStart=1").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedMinimalLevelList result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(30));
        Assert.That(result.Items.Count(s => s.Type == "user"), Is.EqualTo(30));
        Assert.That(result.Items.Count(s => s.Type == "playlist"), Is.Zero);
        Assert.That(result.Users, Has.Count.Zero);
        Assert.That(result.Total, Is.EqualTo(35)); // just levels
        Assert.That(result.NextPageStart, Is.EqualTo(31));
    }
}