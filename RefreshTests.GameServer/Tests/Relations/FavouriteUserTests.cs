using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.UserData;
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

    [Test]
    public void CantFavouriteMissingUser()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        //Favourite an invalid user
        HttpResponseMessage message = client.PostAsync($"/lbp/favourite/user/pain peko", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
        
        //Get the favourite users
        message = client.GetAsync($"/lbp/favouriteUsers/{user.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        //Make sure it is empty
        SerializedFavouriteUserList result = message.Content.ReadAsXML<SerializedFavouriteUserList>();
        Assert.That(result.Items, Has.Count.EqualTo(0)); 
    }
    
    [Test]
    public void CantFavouriteMissingUserPsp()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        client.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");
        
        //Favourite an invalid user
        HttpResponseMessage message = client.PostAsync($"/lbp/favourite/user/painer peko", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //Get the favourite users
        message = client.GetAsync($"/lbp/favouriteUsers/{user.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        //Make sure its still empty
        SerializedFavouriteUserList result = message.Content.ReadAsXML<SerializedFavouriteUserList>();
        Assert.That(result.Items, Has.Count.EqualTo(0)); 
    }
    
    [Test]
    public void CantUnfavouriteMissingUser()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        //Unfavourite an invalid user
        HttpResponseMessage message = client.PostAsync($"/lbp/unfavourite/user/womp womp", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void CantUnfavouriteMissingUserPsp()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        client.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");
        
        //Unfavourite an invalid user
        HttpResponseMessage message = client.PostAsync($"/lbp/unfavourite/user/gymbag", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
    }
    
    [Test]
    public void CantFavouriteUserTwice()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Favourite a user
        HttpResponseMessage message = client.PostAsync($"/lbp/favourite/user/{user2.Username}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //Favourite the same user again
        message = client.PostAsync($"/lbp/favourite/user/{user2.Username}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(Unauthorized));
        
        //Get the favourite users
        message = client.GetAsync($"/lbp/favouriteUsers/{user1.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        //Make sure it has the user
        SerializedFavouriteUserList result = message.Content.ReadAsXML<SerializedFavouriteUserList>();
        Assert.That(result.Items, Has.Count.EqualTo(1)); 
        Assert.That(result.Items.First().Handle.Username, Is.EqualTo(user2.Username));
    }
    
    [Test]
    public void CantFavouriteUserTwicePsp()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);
        client.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");
        
        //Favourite a user
        HttpResponseMessage message = client.PostAsync($"/lbp/favourite/user/{user2.Username}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //Favourite the same user again
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
    
    [Test]
    public void CantUnfavouriteUserTwice()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Unfavourite a user, which we haven't favourited
        HttpResponseMessage message = client.PostAsync($"/lbp/unfavourite/user/{user2.Username}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(Unauthorized));
    }
    
    [Test]
    public void CantUnfavouriteUserTwicePsp()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);
        client.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");
        
        //Unfavourite a user, which we haven't favourited
        HttpResponseMessage message = client.PostAsync($"/lbp/unfavourite/user/{user2.Username}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
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