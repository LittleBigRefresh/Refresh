using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Levels;
using Refresh.Interfaces.APIv3.Endpoints.DataTypes.Response.Users;
using Refresh.Interfaces.Game.Types.Lists;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Categories;

public class SearchCategoryTests : GameServerTest
{
    private void SpamCreateEntities(TestContext context, int levelCount, int userCount)
    {
        GameUser publisher = context.CreateUser("MIKE");
        for (int i = 0; i < levelCount; i++)
        {
            context.CreateLevel(publisher, "some level " + i);
        }
        for (int i = 0; i < userCount; i++)
        {
            context.CreateUser("some_user_" + i);
        }
    }
    
    [Test]
    public void SearchSlotEndpointReturnsLevelsAndUsers()
    {
        using TestContext context = this.GetServer();
        this.SpamCreateEntities(context, 42, 42);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet2, TokenPlatform.PS3, context.CreateUser());
        
        // Page 1
        HttpResponseMessage message = client.GetAsync($"/lbp/slots/search?query=some&pageSize=40&pageStart=1").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedMinimalLevelList result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(40));
        Assert.That(result.Users, Has.Count.EqualTo(40));
        Assert.That(result.Total, Is.EqualTo(84));
        Assert.That(result.NextPageStart, Is.EqualTo(41));
        
        // Page 2
        message = client.GetAsync($"/lbp/slots/search?query=some&pageSize=40&pageStart=41").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(2));
        Assert.That(result.Users, Has.Count.EqualTo(2));
        Assert.That(result.Total, Is.EqualTo(84)); // still the same
        Assert.That(result.NextPageStart, Is.EqualTo(0));
        
        // Now test pagination if no levels but only users
        // Page 1
        message = client.GetAsync($"/lbp/slots/search?query=user&pageSize=40&pageStart=1").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.Zero);
        Assert.That(result.Users, Has.Count.EqualTo(40));
        Assert.That(result.Total, Is.EqualTo(42));
        Assert.That(result.NextPageStart, Is.EqualTo(41));
        
        // Page 2
        message = client.GetAsync($"/lbp/slots/search?query=user&pageSize=40&pageStart=41").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.Zero);
        Assert.That(result.Users, Has.Count.EqualTo(2));
        Assert.That(result.Total, Is.EqualTo(42)); // still the same
        Assert.That(result.NextPageStart, Is.EqualTo(0));
        
        // Now levels but no users
        // Page 1
        message = client.GetAsync($"/lbp/slots/search?query=level&pageSize=40&pageStart=1").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(40));
        Assert.That(result.Users, Has.Count.Zero);
        Assert.That(result.Total, Is.EqualTo(42));
        Assert.That(result.NextPageStart, Is.EqualTo(41));
        
        // Page 2
        message = client.GetAsync($"/lbp/slots/search?query=level&pageSize=40&pageStart=41").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(2));
        Assert.That(result.Users, Has.Count.Zero);
        Assert.That(result.Total, Is.EqualTo(42)); // still the same
        Assert.That(result.NextPageStart, Is.EqualTo(0));
    }
        
    // TODO test hiding entity types using search params once they are implemented
    
    [Test]
    public void ReturningLessUsersInLBP1DoesntBreakPagination()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, publisher);
        
        // This way we can both test pages only having a third of the requested user count (10 instead of the usual 30),
        // and also ensure we can still go to the third page for the last few users, even though there is no third page for levels.
        this.SpamCreateEntities(context, 35, 23);
        
        // Page 1
        HttpResponseMessage message = client.GetAsync($"/lbp/slots/search?query=some&pageSize=30&pageStart=1").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        SerializedMinimalLevelList result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(30));
        Assert.That(result.Users, Has.Count.EqualTo(10));
        Assert.That(result.Total, Is.EqualTo(58)); // all users + levels
        Assert.That(result.NextPageStart, Is.EqualTo(31));
        
        // Page 2
        message = client.GetAsync($"/lbp/slots/search?query=some&pageSize=30&pageStart=31").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(5));
        Assert.That(result.Users, Has.Count.EqualTo(10));
        Assert.That(result.Total, Is.EqualTo(58)); // doesn't change on any following requests
        Assert.That(result.NextPageStart, Is.EqualTo(61)); // ensure it's not 0 even though no more level pages
        
        // Page 3 (no levels, only users)
        message = client.GetAsync($"/lbp/slots/search?query=some&pageSize=30&pageStart=61").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.Zero);
        Assert.That(result.Users, Has.Count.EqualTo(3));
        Assert.That(result.Total, Is.EqualTo(58));
        Assert.That(result.NextPageStart, Is.EqualTo(0));
        
        // Now ensure we will also be able to still paginate if there are levels but no users
        // Page 1
        message = client.GetAsync($"/lbp/slots/search?query=level&pageSize=30&pageStart=1").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(30));
        Assert.That(result.Users, Has.Count.Zero);
        Assert.That(result.Total, Is.EqualTo(35)); // all users + levels
        Assert.That(result.NextPageStart, Is.EqualTo(31));
        
        // Page 2
        message = client.GetAsync($"/lbp/slots/search?query=level&pageSize=30&pageStart=31").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));

        result = message.Content.ReadAsXML<SerializedMinimalLevelList>();
        Assert.That(result.Items, Has.Count.EqualTo(5));
        Assert.That(result.Users, Has.Count.Zero);
        Assert.That(result.Total, Is.EqualTo(35)); // doesn't change on any following requests
        Assert.That(result.NextPageStart, Is.EqualTo(0));
    }
    
    [Test]
    public void SearchCategoryEndpointReturnsLevelsAndUsers()
    {
        using TestContext context = this.GetServer();
        this.SpamCreateEntities(context, 42, 42);
        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet3, TokenPlatform.PS3, context.CreateUser());
        
        HttpResponseMessage message = client.GetAsync($"/lbp/searches/levels/search?textFilter=some&pageSize=40&pageStart=1").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        SerializedCategoryResultsList result = message.Content.ReadAsXML<SerializedCategoryResultsList>();
        Assert.That(result.Levels, Has.Count.EqualTo(40));
        Assert.That(result.Users, Has.Count.EqualTo(40));
        Assert.That(result.Total, Is.EqualTo(84));
        Assert.That(result.NextPageStart, Is.EqualTo(41));
        
        // Page 2
        message = client.GetAsync($"/lbp/searches/levels/search?textFilter=some&pageSize=40&pageStart=41").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        result = message.Content.ReadAsXML<SerializedCategoryResultsList>();

        Assert.That(result.Levels, Has.Count.EqualTo(2));
        Assert.That(result.Users, Has.Count.EqualTo(2));
        Assert.That(result.Total, Is.EqualTo(84)); // still the same
        Assert.That(result.NextPageStart, Is.EqualTo(0));
        
        // Now test pagination if no levels but only users
        // Page 1
        message = client.GetAsync($"/lbp/searches/levels/search?textFilter=user&pageSize=40&pageStart=1").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        result = message.Content.ReadAsXML<SerializedCategoryResultsList>();
        
        Assert.That(result.Levels, Has.Count.Zero);
        Assert.That(result.Users, Has.Count.EqualTo(40));
        Assert.That(result.Total, Is.EqualTo(42));
        Assert.That(result.NextPageStart, Is.EqualTo(41));
        
        // Page 2
        message = client.GetAsync($"/lbp/searches/levels/search?textFilter=user&pageSize=40&pageStart=41").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        result = message.Content.ReadAsXML<SerializedCategoryResultsList>();

        Assert.That(result.Levels, Has.Count.Zero);
        Assert.That(result.Users, Has.Count.EqualTo(2));
        Assert.That(result.Total, Is.EqualTo(42)); // still the same
        Assert.That(result.NextPageStart, Is.EqualTo(0));
        
        // Now levels but no users
        // Page 1
        message = client.GetAsync($"/lbp/searches/levels/search?textFilter=level&pageSize=40&pageStart=1").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        result = message.Content.ReadAsXML<SerializedCategoryResultsList>();

        Assert.That(result.Levels, Has.Count.EqualTo(40));
        Assert.That(result.Total, Is.EqualTo(42));
        Assert.That(result.NextPageStart, Is.EqualTo(41));
        
        // Page 2
        message = client.GetAsync($"/lbp/searches/levels/search?textFilter=level&pageSize=40&pageStart=41").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        result = message.Content.ReadAsXML<SerializedCategoryResultsList>();

        Assert.That(result.Levels, Has.Count.EqualTo(2));
        Assert.That(result.Total, Is.EqualTo(42)); // still the same
        Assert.That(result.NextPageStart, Is.EqualTo(0));
        
        // Can hide them with query params
        // Hide users
        message = client.GetAsync($"/lbp/searches/levels/search?textFilter=some&resultType[]=slot&pageSize=40&pageStart=1").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        result = message.Content.ReadAsXML<SerializedCategoryResultsList>();
        
        Assert.That(result.Levels, Has.Count.EqualTo(40));
        Assert.That(result.Users, Has.Count.Zero);
        Assert.That(result.Total, Is.EqualTo(42));
        Assert.That(result.NextPageStart, Is.EqualTo(41));
        
        // Hide levels
        message = client.GetAsync($"/lbp/searches/levels/search?textFilter=some&resultType[]=user&pageSize=40&pageStart=1").Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        result = message.Content.ReadAsXML<SerializedCategoryResultsList>();
        
        Assert.That(result.Users, Has.Count.EqualTo(40));
        Assert.That(result.Levels, Has.Count.Zero);
        Assert.That(result.Total, Is.EqualTo(42));
        Assert.That(result.NextPageStart, Is.EqualTo(41));
    }
    
    [Test]
    public void ApiReceivesEntitiesFromVariousSearchCategories()
    {
        using TestContext context = this.GetServer();
        this.SpamCreateEntities(context, 42, 44);
        
        // No need to test pagination here since we don't do anything fancy here, such as returning multiple lists at once,
        // or determining which NextPageIndex to use. We can still test the returned pagination data however.
        ApiListResponse<ApiGameLevelResponse>? levels = context.Http.GetList<ApiGameLevelResponse>($"/api/v3/levels/search?query=some&count=40&skip=0");
        Assert.That(levels?.Data, Is.Not.Null);
        Assert.That(levels!.ListInfo, Is.Not.Null);
        
        Assert.That(levels.Data, Has.Count.EqualTo(40));
        Assert.That(levels.ListInfo.TotalItems, Is.EqualTo(42));
        Assert.That(levels.ListInfo.NextPageIndex, Is.EqualTo(41));
        
        // Ensure we can search for users using a separate category
        ApiListResponse<ApiGameUserResponse>? users = context.Http.GetList<ApiGameUserResponse>($"/api/v3/users/search?query=some&count=40&skip=0");
        Assert.That(users?.Data, Is.Not.Null);
        Assert.That(users!.ListInfo, Is.Not.Null);
        
        Assert.That(users.Data, Has.Count.EqualTo(40));
        Assert.That(users.ListInfo.TotalItems, Is.EqualTo(44));
        Assert.That(users.ListInfo.NextPageIndex, Is.EqualTo(41));
    }
}