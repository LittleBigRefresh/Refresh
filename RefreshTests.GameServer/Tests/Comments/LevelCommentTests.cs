using Refresh.Common.Extensions;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Comments;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Lists;
using Refresh.GameServer.Types.UserData;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Comments;

public class LevelCommentTests : GameServerTest
{
    [Test]
    public void PostAndDeleteLevelComment()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        SerializedComment comment = new()
        {
            Content = "This is a test comment!",
            CommentId = 0,
            Timestamp = 0,
            Handle = null,
        };

        HttpResponseMessage response = client.PostAsync($"/lbp/postComment/user/{level.LevelId}", new StringContent(comment.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));

        response = client.GetAsync($"/lbp/comments/user/{level.LevelId}").Result;
        SerializedCommentList userComments = response.Content.ReadAsXml<SerializedCommentList>();
        Assert.That(userComments.Items, Has.Count.EqualTo(1));
        Assert.That(userComments.Items[0].Content, Is.EqualTo(comment.Content));
        
        response = client.PostAsync($"/lbp/deleteComment/user/{level.LevelId}?commentId={userComments.Items[0].CommentId}", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));
        
        response = client.GetAsync($"/lbp/comments/user/{level.LevelId}").Result;
        userComments = response.Content.ReadAsXml<SerializedCommentList>();
        Assert.That(userComments.Items, Has.Count.EqualTo(0));
    }
    
    [Test]
    public void CantPostTooLongLevelComment()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        SerializedComment comment = new()
        {
            Content = new string('S', 5000),
            CommentId = 0,
            Timestamp = 0,
            Handle = null,
        };

        HttpResponseMessage response = client.PostAsync($"/lbp/postComment/user/{level.LevelId}", new StringContent(comment.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(BadRequest));
    }

    [Test]
    public void CantPostCommentToInvalidLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        SerializedComment comment = new()
        {
            Content = "This is a test comment!",
            CommentId = 0,
            Timestamp = 0,
            Handle = null,
        };

        HttpResponseMessage response = client.PostAsync($"/lbp/postComment/user/I_AM_NOT_REAL", new StringContent(comment.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void CantGetLevelCommentsOfInvalidLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage response = client.GetAsync($"/lbp/comments/user/I_AM_NOT_REAL").Result;
        Assert.That(response.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void CantDeleteInvalidLevelCommentId()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage response = client.PostAsync($"/lbp/deleteComment/user/{level.LevelId}?commentId=BAD", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(BadRequest));
    }
    
    [Test]
    public void CantDeleteCommentForInvalidLevel()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage response = client.PostAsync($"/lbp/deleteComment/user/I_AM_NOT_REAL?commentId=1", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void CantDeleteNonExistentLevelComment()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage response = client.PostAsync($"/lbp/deleteComment/user/{level.LevelId}?commentId=1", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(BadRequest));
    }
    
    [Test]
    public void CantDeleteAnotherUsersComment()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameLevel level = context.CreateLevel(user1);

        using HttpClient client1 = context.GetAuthenticatedClient(TokenType.Game, user1);
        using HttpClient client2 = context.GetAuthenticatedClient(TokenType.Game, user2);

        SerializedComment comment = new()
        {
            Content = "This is a test comment!",
            CommentId = 0,
            Timestamp = 0,
            Handle = null,
        };

        HttpResponseMessage response = client1.PostAsync($"/lbp/postComment/user/{level.LevelId}", new StringContent(comment.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));

        response = client1.GetAsync($"/lbp/comments/user/{level.LevelId}").Result;
        SerializedCommentList userComments = response.Content.ReadAsXml<SerializedCommentList>();
        Assert.That(userComments.Items, Has.Count.EqualTo(1));
        Assert.That(userComments.Items[0].Content, Is.EqualTo(comment.Content));
        
        response = client2.PostAsync($"/lbp/deleteComment/user/{level.LevelId}?commentId={userComments.Items[0].CommentId}", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(Unauthorized));
    }

    [Test]
    public void RateUserLevelComment()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.CreateLevel(user);
        GameLevelComment comment = context.Database.PostCommentToLevel(level, user, "This is a test comment!");
        
        CommentTests.RateComment(context, user, comment, $"/lbp/rateComment/user/{level.LevelId}", $"/lbp/comments/user/{level.LevelId}");
    }
    
    [Test]
    public void RateDeveloperLevelComment()
    {
        const int levelId = 1;
        
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameLevel level = context.Database.GetStoryLevelById(levelId);
        GameLevelComment comment = context.Database.PostCommentToLevel(level, user, "This is a test comment!");
        
        CommentTests.RateComment(context, user, comment, $"/lbp/rateComment/developer/{level.LevelId}", $"/lbp/comments/developer/{level.LevelId}");
    }
}