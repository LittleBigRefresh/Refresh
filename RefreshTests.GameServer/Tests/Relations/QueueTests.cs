using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.UserData;
using RefreshTests.GameServer.Extensions;

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
        Assert.That(user.QueueLevelRelations.Count(), Is.EqualTo(1));
        Assert.That(user.QueueLevelRelations.First().Level.LevelId, Is.EqualTo(level.LevelId));
        
        //Remove the level from the queue
        message = client.PostAsync($"/lbp/lolcatftw/remove/user/{level.LevelId}", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        context.Database.Refresh();
        //Ensure the queue is cleared
        Assert.That(user.QueueLevelRelations.Count(), Is.EqualTo(0));
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
        Assert.That(user.QueueLevelRelations.Count(), Is.EqualTo(2));
        Assert.That(user.QueueLevelRelations.AsEnumerable().Any(q => q.Level.LevelId == level1.LevelId));
        Assert.That(user.QueueLevelRelations.AsEnumerable().Any(q => q.Level.LevelId == level2.LevelId));
        
        //Clear the queue
        message = client.PostAsync($"/lbp/lolcatftw/clear", new ReadOnlyMemoryContent(Array.Empty<byte>())).Result;
        Assert.That(message.StatusCode, Is.EqualTo(OK));
        
        context.Database.Refresh();
        //Ensure the queue is cleared
        Assert.That(user.QueueLevelRelations.Count(), Is.EqualTo(0));
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
        Assert.That(user.QueueLevelRelations.Count(), Is.EqualTo(1));
        Assert.That(user.QueueLevelRelations.First().Level.LevelId, Is.EqualTo(level.LevelId));
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
        Assert.That(user.QueueLevelRelations.Count(), Is.EqualTo(0));
    }
}