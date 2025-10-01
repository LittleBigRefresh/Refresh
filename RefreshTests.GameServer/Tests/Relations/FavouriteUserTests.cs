using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Types.Lists;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Relations;

public class FavouriteUserTests : GameServerTest
{
    [Test]
    public void FavouriteAndUnfavouriteUser()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Favourite a user
        HttpResponseMessage message = client.PostAsync($"/lbp/favourite/user/{user2.Username}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //Get the favourite users now
        message = client.GetAsync($"/lbp/favouriteUsers/{user1.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        //Make sure the only entry is the user we favourited
        SerializedFavouriteUserList result = message.Content.ReadAsXML<SerializedFavouriteUserList>();
        Assert.That(result.Items, Has.Count.EqualTo(1));
        Assert.That(result.Items.First().Handle.Username, Is.EqualTo(user2.Username));

        //Unfavourite the user
        message = client.PostAsync($"/lbp/unfavourite/user/{user2.Username}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //Get the favourite users
        message = client.GetAsync($"/lbp/favouriteUsers/{user1.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        //Make sure its now empty
        result = message.Content.ReadAsXML<SerializedFavouriteUserList>();
        Assert.That(result.Items, Has.Count.EqualTo(0));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void CantFavouriteMissingUser(bool psp)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        if(psp)
            client.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");

        //Favourite an invalid user
        HttpResponseMessage message = client.PostAsync($"/lbp/favourite/user/pain peko", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(psp ? OK : NotFound));
        
        //Get the favourite users
        message = client.GetAsync($"/lbp/favouriteUsers/{user.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        //Make sure it is empty
        SerializedFavouriteUserList result = message.Content.ReadAsXML<SerializedFavouriteUserList>();
        Assert.That(result.Items, Has.Count.EqualTo(0)); 
    }
    
    [TestCase(false)]
    [TestCase(true)]
    public void CantUnfavouriteMissingUser(bool psp)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        if(psp)
            client.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");

        //Unfavourite an invalid user
        HttpResponseMessage message = client.PostAsync($"/lbp/unfavourite/user/womp womp", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(psp ? OK : NotFound));
    }
    
    [TestCase(false)]
    [TestCase(true)]
    public void CanTryToFavouriteUserTwice(bool psp)
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);
        if (psp)
            client.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");
        
        //Favourite a user
        HttpResponseMessage message = client.PostAsync($"/lbp/favourite/user/{user2.Username}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //Favourite the same user again (should still stay favourited)
        message = client.PostAsync($"/lbp/favourite/user/{user2.Username}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //Get the favourite users
        message = client.GetAsync($"/lbp/favouriteUsers/{user1.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        //Make sure it has the user
        SerializedFavouriteUserList result = message.Content.ReadAsXML<SerializedFavouriteUserList>();
        Assert.That(result.Items, Has.Count.EqualTo(1)); 
        Assert.That(result.Items.First().Handle.Username, Is.EqualTo(user2.Username));
    }
    
    [TestCase(false)]
    [TestCase(true)]
    public void CanTryToUnfavouriteUserTwice(bool psp)
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);
        if(psp)        
            client.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");

        //Unfavourite a user, which we haven't favourited
        HttpResponseMessage message = client.PostAsync($"/lbp/unfavourite/user/{user2.Username}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        //Make sure they're still unfavourited
        Assert.That(context.Database.IsUserFavouritedByUser(user2, user1), Is.False);
    }
    
    [Test]
    public void CantGetFavouriteUsersForInvalidUser()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        
        //Get the favourite users for an invalid user
        HttpResponseMessage message = client.GetAsync("/lbp/favouriteUsers/WHO ARE YOU?").Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }
}