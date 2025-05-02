using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;

namespace RefreshTests.GameServer.Tests.Levels;

public class ReviewEndpointsTests : GameServerTest
{
    [Test]
    public void DpadRateLevelBoo()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/dpadrate/user/{level.LevelId}?rating={(sbyte)RatingType.Boo}", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        context.Database.Refresh();
        Assert.That(context.Database.GetRatingByUser(level, user1), Is.EqualTo(RatingType.Boo));
        Assert.That(context.Database.GetRatingByUser(level, user2), Is.EqualTo(null));
    } 
    
    [Test]
    public void DpadRateLevelNeutral()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/dpadrate/user/{level.LevelId}?rating={(sbyte)RatingType.Neutral}", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        context.Database.Refresh();
        Assert.That(context.Database.GetRatingByUser(level, user1), Is.EqualTo(RatingType.Neutral));
        Assert.That(context.Database.GetRatingByUser(level, user2), Is.EqualTo(null));
    } 
    
    [Test]
    public void DpadRateLevelYay()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/dpadrate/user/{level.LevelId}?rating={(sbyte)RatingType.Yay}", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        context.Database.Refresh();
        Assert.That(context.Database.GetRatingByUser(level, user1), Is.EqualTo(RatingType.Yay));
        Assert.That(context.Database.GetRatingByUser(level, user2), Is.EqualTo(null));
    } 
    
    [Test]
    public void DpadRateLevelOverwrite()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/dpadrate/user/{level.LevelId}?rating={(sbyte)RatingType.Yay}", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        context.Database.Refresh();
        Assert.That(context.Database.GetRatingByUser(level, user1), Is.EqualTo(RatingType.Yay));
        Assert.That(context.Database.GetRatingByUser(level, user2), Is.EqualTo(null));
        
        message = client.PostAsync($"/lbp/dpadrate/user/{level.LevelId}?rating={(sbyte)RatingType.Neutral}", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        context.Database.Refresh();
        Assert.That(context.Database.GetRatingByUser(level, user1), Is.EqualTo(RatingType.Neutral));
        Assert.That(context.Database.GetRatingByUser(level, user2), Is.EqualTo(null));
    } 
    
    [Test]
    public void CantDpadRateLevelWithInvalidRating()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        context.Database.PlayLevel(level, user1, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/dpadrate/user/{level.LevelId}?rating=YAY", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
        
        message = client.PostAsync($"/lbp/dpadrate/user/{level.LevelId}?rating=2", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    } 

    [Test]
    public void CantDpadRateInvalidLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message = client.PostAsync($"/lbp/dpadrate/user/{int.MaxValue}?rating=1", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    } 
    
    [Test]
    public void RateLevel1Star()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/rate/user/{level.LevelId}?rating=1", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        context.Database.Refresh();
        Assert.That(context.Database.GetRatingByUser(level, user1), Is.EqualTo(RatingType.Boo));
        Assert.That(context.Database.GetRatingByUser(level, user2), Is.EqualTo(null));
    } 
    
    [Test]
    public void RateLevel2Star()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/rate/user/{level.LevelId}?rating=2", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        context.Database.Refresh();
        Assert.That(context.Database.GetRatingByUser(level, user1), Is.EqualTo(RatingType.Boo));
        Assert.That(context.Database.GetRatingByUser(level, user2), Is.EqualTo(null));
    } 
    
    [Test]
    public void RateLevel3Star()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/rate/user/{level.LevelId}?rating=3", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        context.Database.Refresh();
        Assert.That(context.Database.GetRatingByUser(level, user1), Is.EqualTo(RatingType.Neutral));
        Assert.That(context.Database.GetRatingByUser(level, user2), Is.EqualTo(null));
    } 
    
    [Test]
    public void RateLevel4Star()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/rate/user/{level.LevelId}?rating=4", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        context.Database.Refresh();
        Assert.That(context.Database.GetRatingByUser(level, user1), Is.EqualTo(RatingType.Yay));
        Assert.That(context.Database.GetRatingByUser(level, user2), Is.EqualTo(null));
    } 
    
    [Test]
    public void RateLevel5Star()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/rate/user/{level.LevelId}?rating=5", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        context.Database.Refresh();
        Assert.That(context.Database.GetRatingByUser(level, user1), Is.EqualTo(RatingType.Yay));
        Assert.That(context.Database.GetRatingByUser(level, user2), Is.EqualTo(null));
    } 
    
    [Test]
    public void CantRateInvalidLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message = client.PostAsync($"/lbp/rate/user/{int.MaxValue}?rating=1", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound));
    } 
    
    [Test]
    public void CantRateLevelWithInvalidRating()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user2);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        //Play the level at least once
        context.Database.PlayLevel(level, user1, 1);
        
        HttpResponseMessage message = client.PostAsync($"/lbp/rate/user/{level.LevelId}?rating=WHAT", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
        
        message = client.PostAsync($"/lbp/rate/user/{level.LevelId}?rating=6", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(BadRequest));
    } 
}