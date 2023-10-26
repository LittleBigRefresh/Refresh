using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.UserData;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Relations;

public class FavouriteSlotTests : GameServerTest
{
    [Test]
    public void FavouriteAndUnfavouriteLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        //Favourite a level
        HttpResponseMessage message = client.PostAsync($"/lbp/favourite/slot/user/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //Get the favourite slots now
        message = client.GetAsync($"/lbp/favouriteSlots/{user.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        //Make sure the only entry is the level
        SerializedMinimalFavouriteLevelList result = message.Content.ReadAsXML<SerializedMinimalFavouriteLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(1));
        Assert.That(result.Items.First().LevelId, Is.EqualTo(level.LevelId));

        //Unfavourite the level
        message = client.PostAsync($"/lbp/unfavourite/slot/user/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //Get the favourite slots
        message = client.GetAsync($"/lbp/favouriteSlots/{user.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        //Make sure its now empty
        result = message.Content.ReadAsXML<SerializedMinimalFavouriteLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(0));
    }

    [Test]
    public void CantFavouriteMissingLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        //Favourite an invalid level
        HttpResponseMessage message = client.PostAsync($"/lbp/favourite/slot/user/{int.MaxValue}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
        
        //Get the favourite slots
        message = client.GetAsync($"/lbp/favouriteSlots/{user.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        //Make sure its now empty
        SerializedMinimalFavouriteLevelList result = message.Content.ReadAsXML<SerializedMinimalFavouriteLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(0)); 
    }
    
    [Test]
    public void CantFavouriteMissingLevelPsp()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        client.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");
        
        //Favourite an invalid level
        HttpResponseMessage message = client.PostAsync($"/lbp/favourite/slot/user/{int.MaxValue}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //Get the favourite slots
        message = client.GetAsync($"/lbp/favouriteSlots/{user.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        //Make sure its now empty
        SerializedMinimalFavouriteLevelList result = message.Content.ReadAsXML<SerializedMinimalFavouriteLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(0)); 
    }
    
    [Test]
    public void CantUnfavouriteMissingLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        //Unfavourite an invalid level
        HttpResponseMessage message = client.PostAsync($"/lbp/unfavourite/slot/user/{int.MaxValue}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void CantUnfavouriteMissingLevelPsp()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        client.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");
        
        //Unfavourite an invalid level
        HttpResponseMessage message = client.PostAsync($"/lbp/unfavourite/slot/user/{int.MaxValue}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
    }
    
    [Test]
    public void CantFavouriteLevelTwice()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        //Favourite a level
        HttpResponseMessage message = client.PostAsync($"/lbp/favourite/slot/user/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //Favourite another level
        message = client.PostAsync($"/lbp/favourite/slot/user/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(Unauthorized));
        
        //Get the favourite slots
        message = client.GetAsync($"/lbp/favouriteSlots/{user.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        //Make sure it has the level
        SerializedMinimalFavouriteLevelList result = message.Content.ReadAsXML<SerializedMinimalFavouriteLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(1)); 
        Assert.That(result.Items.First().LevelId, Is.EqualTo(level.LevelId));
    }
    
    [Test]
    public void CantFavouriteLevelTwicePsp()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        client.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");
        
        //Favourite a level
        HttpResponseMessage message = client.PostAsync($"/lbp/favourite/slot/user/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //Favourite the level again
        message = client.PostAsync($"/lbp/favourite/slot/user/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //Get the favourite slots
        message = client.GetAsync($"/lbp/favouriteSlots/{user.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        //Make sure it has the level
        SerializedMinimalFavouriteLevelList result = message.Content.ReadAsXML<SerializedMinimalFavouriteLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(1)); 
        Assert.That(result.Items.First().LevelId, Is.EqualTo(level.LevelId));
    }
    
    [Test]
    public void CantUnfavouriteLevelTwice()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        //Unfavourite a level, which we haven't favourited
        HttpResponseMessage message = client.PostAsync($"/lbp/unfavourite/slot/user/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(Unauthorized));
    }
    
    [Test]
    public void CantUnfavouriteLevelTwicePsp()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        client.DefaultRequestHeaders.UserAgent.TryParseAdd("LBPPSP CLIENT");
        
        //Unfavourite a level, which we haven't favourited
        HttpResponseMessage message = client.PostAsync($"/lbp/unfavourite/slot/user/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
    }
}