using Refresh.Database.Query;
using Refresh.Database.Models.Authentication;
using Refresh.Database;
using Refresh.Database.Models.Users;
using Refresh.Database.Models.Levels;

namespace RefreshTests.GameServer.Tests.Relations;

public class QueueTests : GameServerTest
{
    [Test]
    public void QueueAndUnqueueLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        //Queue a level
        HttpResponseMessage message = client.PostAsync($"/lbp/lolcatftw/add/user/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        context.Database.Refresh();
        //Ensure we only have one queued level, and that it is the level we queued
        Assert.That(context.Database.GetTotalLevelsQueuedByUser(user), Is.EqualTo(1));
        Assert.That(context.Database.GetLevelsQueuedByUser(user, 1, 0, new(TokenGame.LittleBigPlanet2), user)
            .Items.First().LevelId, Is.EqualTo(level.LevelId));
        
        //Remove the level from the queue
        message = client.PostAsync($"/lbp/lolcatftw/remove/user/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        context.Database.Refresh();
        //Ensure the queue is cleared
        Assert.That(context.Database.GetTotalLevelsQueuedByUser(user), Is.EqualTo(0));
    }
    
    [Test]
    public void ClearQueue()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level1 = context.CreateLevel(user);
        GameLevel level2 = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        //Queue 2 levels
        HttpResponseMessage message = client.PostAsync($"/lbp/lolcatftw/add/user/{level1.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        message = client.PostAsync($"/lbp/lolcatftw/add/user/{level2.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        context.Database.Refresh();
        //Ensure we have both levels queued
        Assert.That(context.Database.GetTotalLevelsQueuedByUser(user), Is.EqualTo(2));

        DatabaseList<GameLevel> queue = context.Database.GetLevelsQueuedByUser(user, 2, 0,
            new(TokenGame.LittleBigPlanet2), user);
        
        Assert.That(queue.Items.Any(q => q.LevelId == level1.LevelId));
        Assert.That(queue.Items.Any(q => q.LevelId == level2.LevelId));
        
        //Clear the queue
        message = client.PostAsync($"/lbp/lolcatftw/clear", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        context.Database.Refresh();
        //Ensure the queue is cleared
        Assert.That(context.Database.GetTotalLevelsQueuedByUser(user), Is.EqualTo(0));
    }

    [Test]
    public void CantQueueInvalidLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message = client.PostAsync($"/lbp/lolcatftw/add/user/{int.MaxValue}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound)); 
    }
    
    [Test]
    public void CantUnqueueInvalidLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message = client.PostAsync($"/lbp/lolcatftw/remove/user/{int.MaxValue}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(NotFound)); 
    }
    
    [Test]
    public void CantQueueLevelTwice()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message = client.PostAsync($"/lbp/lolcatftw/add/user/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK)); 
        message = client.PostAsync($"/lbp/lolcatftw/add/user/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(Unauthorized)); 
        
        context.Database.Refresh();
        //Ensure we only have one queued level, and that it is the level we queued
        Assert.That(context.Database.GetTotalLevelsQueuedByUser(user), Is.EqualTo(1));
        Assert.That(context.Database.GetLevelsQueuedByUser(user, 1, 0, new(TokenGame.LittleBigPlanet2), user)
            .Items.First().LevelId, Is.EqualTo(level.LevelId));
    }
    
    [Test]
    public void CantDequeueLevelTwice()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage message = client.PostAsync($"/lbp/lolcatftw/remove/user/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(Unauthorized)); 
        
        context.Database.Refresh();
        //Ensure we have 0 queued levels
        Assert.That(context.Database.GetTotalLevelsQueuedByUser(user), Is.EqualTo(0));
    }
}