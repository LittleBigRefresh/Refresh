using Refresh.GameServer.Authentication;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.Reviews;
using Refresh.GameServer.Types.UserData;

namespace RefreshTests.GameServer.Tests.Levels;

public class LevelTests : GameServerTest
{
    [Test]
    public void SlotsNewest()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        
        HttpResponseMessage message = client.GetAsync($"/lbp/slots/newest").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedMinimalLevelList result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(1));
        Assert.That(result.Items.First().LevelId, Is.EqualTo(level.LevelId));
        
        //slots without a route is equivalent to newest
        message = client.GetAsync($"/lbp/slots").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(1));
        Assert.That(result.Items.First().LevelId, Is.EqualTo(level.LevelId));
    }
    
    [Test]
    public void SlotsRandom()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        
        HttpResponseMessage message = client.GetAsync($"/lbp/slots/lbp2luckydip").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedMinimalLevelList result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(1));
        Assert.That(result.Items.First().LevelId, Is.EqualTo(level.LevelId));
    }
    
    [Test]
    public void SlotsQueued()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        //Get the queued slots
        HttpResponseMessage message = client.GetAsync($"/lbp/slots/lolcatftw").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedMinimalLevelList result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(0));
        
        //Add the level to the queue
        message = client.PostAsync($"/lbp/lolcatftw/add/user/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //Get the queued slots now
        message = client.GetAsync($"/lbp/slots/lolcatftw").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(1));
        Assert.That(result.Items.First().LevelId, Is.EqualTo(level.LevelId));
        
        //Remove the level from the queue
        message = client.PostAsync($"/lbp/lolcatftw/remove/user/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //Get the queued slots
        message = client.GetAsync($"/lbp/slots/lolcatftw").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(0));
    }
    
    [Test]
    public void SlotsHearted()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        
        //Get the favourite slots
        HttpResponseMessage message = client.GetAsync($"/lbp/slots/favouriteSlots").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        //Make sure its empty
        SerializedMinimalLevelList result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(0));

        //Favourite the level
        message = client.PostAsync($"/lbp/favourite/slot/user/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //Get the favourite slots now
        message = client.GetAsync($"/lbp/slots/favouriteSlots").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        //Make sure the only entry is the level
        result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(1));
        Assert.That(result.Items.First().LevelId, Is.EqualTo(level.LevelId));

        //Unfavourite the level
        message = client.PostAsync($"/lbp/unfavourite/slot/user/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //Get the favourite slots
        message = client.GetAsync($"/lbp/slots/favouriteSlots").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        //Make sure its now empty
        result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(0));
    }
    
    [Test]
    public void SlotsHeartedQuirk()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        //Get the favourite slots
        HttpResponseMessage message = client.GetAsync($"/lbp/favouriteSlots/{user.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        //Make sure its empty
        SerializedMinimalFavouriteLevelList result = message.Content.ReadAsXML<SerializedMinimalFavouriteLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(0));

        //Favourite the level
        message = client.PostAsync($"/lbp/favourite/slot/user/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //Get the favourite slots now
        message = client.GetAsync($"/lbp/favouriteSlots/{user.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        //Make sure the only entry is the level
        result = message.Content.ReadAsXML<SerializedMinimalFavouriteLevelList>();
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
    public void SlotsHeartedQuirkFailsWhenInvalidUser()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        
        //Get the favourite slots
        HttpResponseMessage message = client.GetAsync($"/lbp/favouriteSlots/I_AM_NOT_A_REAL_USER").Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void SlotsMostHearted()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameUser user3 = context.CreateUser();
        GameLevel level = context.CreateLevel(user);
        GameLevel level2 = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        
        HttpResponseMessage message = client.GetAsync($"/lbp/slots/mostHearted").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedMinimalLevelList result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(0));

        context.Database.FavouriteLevel(level, user);
        context.Database.FavouriteLevel(level, user2);
        context.Database.FavouriteLevel(level2, user2);
        
        message = client.GetAsync($"/lbp/slots/mostHearted").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(2));
        Assert.That(result.Items[0].LevelId, Is.EqualTo(level.LevelId));
        Assert.That(result.Items[1].LevelId, Is.EqualTo(level2.LevelId));

        context.Database.FavouriteLevel(level2, user);
        context.Database.FavouriteLevel(level2, user3);
        
        message = client.GetAsync($"/lbp/slots/mostHearted").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(2));
        Assert.That(result.Items[0].LevelId, Is.EqualTo(level2.LevelId));
        Assert.That(result.Items[1].LevelId, Is.EqualTo(level.LevelId));
    }
    
    [Test]
    public void SlotsMostLiked()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);
        GameLevel level2 = context.CreateLevel(publisher);
        
        GameUser user = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameUser user3 = context.CreateUser();
        
        context.Database.PlayLevel(level, user, 1);
        context.Database.PlayLevel(level2, user, 1);
        context.Database.PlayLevel(level, user2, 1);
        context.Database.PlayLevel(level2, user2, 1);
        context.Database.PlayLevel(level, user3, 1);
        context.Database.PlayLevel(level2, user3, 1);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        
        HttpResponseMessage message = client.GetAsync($"/lbp/slots/highestRated").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedMinimalLevelList result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(0));

        bool a = context.Database.RateLevel(level, user, RatingType.Yay);
        context.Database.RateLevel(level, user2, RatingType.Yay);
        context.Database.RateLevel(level2, user3, RatingType.Yay);

        message = client.GetAsync($"/lbp/slots/highestRated").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(2));
        Assert.That(result.Items[0].LevelId, Is.EqualTo(level.LevelId));
        Assert.That(result.Items[1].LevelId, Is.EqualTo(level2.LevelId));

        context.Database.RateLevel(level2, user, RatingType.Yay);
        context.Database.RateLevel(level, user3, RatingType.Boo);
 
        message = client.GetAsync($"/lbp/slots/highestRated").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(2));
        Assert.That(result.Items[0].LevelId, Is.EqualTo(level2.LevelId));
        Assert.That(result.Items[1].LevelId, Is.EqualTo(level.LevelId));
    }
    
    [Test]
    public void SlotsMostPlayed()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);
        GameLevel level2 = context.CreateLevel(publisher);
        
        GameUser user = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameUser user3 = context.CreateUser();
        
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        
        HttpResponseMessage message = client.GetAsync($"/lbp/slots/mostUniquePlays").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedMinimalLevelList result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(0));
        
        context.Database.PlayLevel(level, user, 1);
        context.Database.PlayLevel(level, user2, 1);
        context.Database.PlayLevel(level2, user2, 1);
        context.Database.PlayLevel(level, user3, 1);
        context.Database.PlayLevel(level2, user3, 1);
        
        message = client.GetAsync($"/lbp/slots/mostUniquePlays").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(2));
        Assert.That(result.Items[0].LevelId, Is.EqualTo(level.LevelId));
        Assert.That(result.Items[1].LevelId, Is.EqualTo(level2.LevelId));
    }
        
    [Test]
    public void SlotsMostReplayed()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);
        GameLevel level2 = context.CreateLevel(publisher);
        
        GameUser user = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameUser user3 = context.CreateUser();
        
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        
        HttpResponseMessage message = client.GetAsync($"/lbp/slots/mostPlays").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedMinimalLevelList result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(0));
        
        context.Database.PlayLevel(level, user, 1);
        context.Database.PlayLevel(level, user2, 1);
        context.Database.PlayLevel(level2, user2, 5);
        context.Database.PlayLevel(level, user3, 1);
        context.Database.PlayLevel(level2, user3, 1);
        
        message = client.GetAsync($"/lbp/slots/mostPlays").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(2));
        Assert.That(result.Items[0].LevelId, Is.EqualTo(level2.LevelId));
        Assert.That(result.Items[1].LevelId, Is.EqualTo(level.LevelId));
    }
    
    [Test]
    public void SlotsTeamPicked()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);
        
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, publisher);
        
        HttpResponseMessage message = client.GetAsync($"/lbp/slots/mmpicks").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedMinimalLevelList result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(0));
        
        context.Database.AddTeamPickToLevel(level);
        
        message = client.GetAsync($"/lbp/slots/mmpicks").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(1));
        Assert.That(result.Items[0].LevelId, Is.EqualTo(level.LevelId));
    }
    
    [Test]
    public void SlotsByUser()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, publisher);
        
        HttpResponseMessage message = client.GetAsync($"/lbp/slots/by/{publisher.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedMinimalLevelList result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(0));

        GameLevel level = context.CreateLevel(publisher);

        message = client.GetAsync($"/lbp/slots/by/{publisher.Username}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(1));
        Assert.That(result.Items[0].LevelId, Is.EqualTo(level.LevelId));
        
        message = client.GetAsync($"/lbp/slots/by/I_AM_NOT_REAL").Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void GetLevelById()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);
        
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, publisher);
        
        HttpResponseMessage message = client.GetAsync($"/lbp/s/user/{level.LevelId}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        GameLevelResponse result = message.Content.ReadAsXML<GameLevelResponse>();
        Assert.That(result.LevelId, Is.EqualTo(level.LevelId));

        message = client.GetAsync($"/lbp/s/user/{int.MaxValue}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void GetSlotList()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);
        GameLevel level2 = context.CreateLevel(publisher);
        
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, publisher);
        
        HttpResponseMessage message = client.GetAsync($"/lbp/slotList?s={level.LevelId}&s={level2.LevelId}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedLevelList result = message.Content.ReadAsXML<SerializedLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(2));
        Assert.That(result.Items[0].LevelId, Is.EqualTo(level.LevelId));
        Assert.That(result.Items[1].LevelId, Is.EqualTo(level2.LevelId));
    }
    
    [Test]
    public void GetLevelsFromCategory()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameLevel level = context.CreateLevel(publisher);
        GameLevel level2 = context.CreateLevel(publisher);
        
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, publisher);
        
        HttpResponseMessage message = client.GetAsync($"/lbp/searches/newest").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedMinimalLevelResultsList result = message.Content.ReadAsXML<SerializedMinimalLevelResultsList>();
        Assert.That(result.Items, Has.Count.EqualTo(2));
        Assert.That(result.Items[0].LevelId, Is.EqualTo(level.LevelId));
        Assert.That(result.Items[1].LevelId, Is.EqualTo(level2.LevelId));
    }
    
    [Test]
    public void DoesntGetSlotListWhenNoQuery()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, publisher);
        
        HttpResponseMessage message = client.GetAsync($"/lbp/slotList").Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    } 
    
    [Test]
    public void DoesntGetSlotListWhenInvalidQuery()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, publisher);
        
        HttpResponseMessage message = client.GetAsync($"/lbp/slotList?s=NOT_A_NUMBER").Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    }

    [Test]
    public void GetSlotListWhenInvalidLevel()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, publisher);

        HttpResponseMessage message = client.GetAsync($"/lbp/slotList?s={int.MaxValue}").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        SerializedLevelList result = message.Content.ReadAsXML<SerializedLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(0));
    }
    
    [Test]
    public void GetModernCategories()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, publisher);

        HttpResponseMessage message = client.GetAsync($"/lbp/genres").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //Just throw away the value, but at least make sure it parses
        _ = message.Content.ReadAsXML<SerializedCategoryList>();
        
        message = client.GetAsync($"/lbp/searches").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        //Just throw away the value, but at least make sure it parses
        _ = message.Content.ReadAsXML<SerializedCategoryList>();
    }

    [Test]
    public void SlotsInvalidRoute()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);
        
        HttpResponseMessage message = client.GetAsync($"/lbp/slots/waaaaaa").Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void SlotsHeartedApiV3FailsWhenNoUser()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetUnauthenticatedClient();
        
        HttpResponseMessage message = client.GetAsync($"/api/v3/levels/hearted").Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    }
}