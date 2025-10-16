using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Game.Types.Comments;
using Refresh.Interfaces.Game.Types.Lists;
using RefreshTests.GameServer.Extensions;

namespace RefreshTests.GameServer.Tests.Comments;

public class UserCommentTests : GameServerTest
{
    [Test]
    public void PostAndDeleteUserComment()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        SerializedComment comment = new()
        {
            Content = "This is a test comment!",
            CommentId = 0,
            Timestamp = 0,
            Handle = null,
        };

        HttpResponseMessage response = client.PostAsync($"/lbp/postUserComment/{user2.Username}", new StringContent(comment.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));

        response = client.GetAsync($"/lbp/userComments/{user2.Username}").Result;
        SerializedCommentList userComments = response.Content.ReadAsXML<SerializedCommentList>();
        Assert.That(userComments.Items, Has.Count.EqualTo(1));
        Assert.That(userComments.Items[0].Content, Is.EqualTo(comment.Content));
        
        response = client.PostAsync($"/lbp/deleteUserComment/{user2.Username}?commentId={userComments.Items[0].CommentId}", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));
        
        response = client.GetAsync($"/lbp/userComments/{user2.Username}").Result;
        userComments = response.Content.ReadAsXML<SerializedCommentList>();
        Assert.That(userComments.Items, Has.Count.EqualTo(0));
    }
    
    [Test]
    public void CantPostTooLongUserComment()
    {
        using TestContext context = this.GetServer();
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user1);

        SerializedComment comment = new()
        {
            Content = new string('S', 5000),
            CommentId = 0,
            Timestamp = 0,
            Handle = null,
        };

        HttpResponseMessage response = client.PostAsync($"/lbp/postUserComment/{user2.Username}", new StringContent(comment.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(BadRequest));
    }

    [Test]
    public void CantUserCommentToInvalidUser()
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

        HttpResponseMessage response = client.PostAsync($"/lbp/postUserComment/I_AM_NOT_REAL", new StringContent(comment.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void CantGetUserCommentsOfInvalidUser()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage response = client.GetAsync($"/lbp/userComments/I_AM_NOT_REAL").Result;
        Assert.That(response.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void CantDeleteInvalidCommentId()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage response = client.PostAsync($"/lbp/deleteUserComment/I_AM_NOT_REAL?commentId=BAD", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(BadRequest));
    }
    
    [Test]
    public void CantDeleteCommentForInvalidUser()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage response = client.PostAsync($"/lbp/deleteUserComment/I_AM_NOT_REAL?commentId=1", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(NotFound));
    }
    
    [Test]
    public void CantDeleteNonExistantComment()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();

        using HttpClient client = context.GetAuthenticatedClient(TokenType.Game, user);

        HttpResponseMessage response = client.PostAsync($"/lbp/deleteUserComment/{user.Username}?commentId=1", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(BadRequest));
    }
    
    [Test]
    public void CantDeleteAnotherUsersComment()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameUser profile = context.CreateUser();
        GameUser moron = context.CreateUser();

        using HttpClient client1 = context.GetAuthenticatedClient(TokenType.Game, publisher);
        using HttpClient client2 = context.GetAuthenticatedClient(TokenType.Game, moron);

        SerializedComment comment = new()
        {
            Content = "This is a test comment!",
            CommentId = 0,
            Timestamp = 0,
            Handle = null,
        };

        HttpResponseMessage response = client1.PostAsync($"/lbp/postUserComment/{profile.Username}", new StringContent(comment.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));

        response = client1.GetAsync($"/lbp/userComments/{profile.Username}").Result;
        SerializedCommentList userComments = response.Content.ReadAsXML<SerializedCommentList>();
        Assert.That(userComments.Items, Has.Count.EqualTo(1));
        Assert.That(userComments.Items[0].Content, Is.EqualTo(comment.Content));
        
        response = client2.PostAsync($"/lbp/deleteUserComment/{profile.Username}?commentId={userComments.Items[0].CommentId}", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(Unauthorized));

        // Make sure the comment is still there
        response = client1.GetAsync($"/lbp/userComments/{profile.Username}").Result;
        userComments = response.Content.ReadAsXML<SerializedCommentList>();
        Assert.That(userComments.Items, Has.Count.EqualTo(1));
        Assert.That(userComments.Items[0].Content, Is.EqualTo(comment.Content));
    }

    [Test]
    public void CanDeleteAnotherUsersCommentAsProfileOwner()
    {
        using TestContext context = this.GetServer();
        GameUser publisher = context.CreateUser();
        GameUser profile = context.CreateUser();

        using HttpClient client1 = context.GetAuthenticatedClient(TokenType.Game, publisher);
        using HttpClient client2 = context.GetAuthenticatedClient(TokenType.Game, profile);

        SerializedComment comment = new()
        {
            Content = "This is a test comment!",
            CommentId = 0,
            Timestamp = 0,
            Handle = null,
        };

        HttpResponseMessage response = client1.PostAsync($"/lbp/postUserComment/{profile.Username}", new StringContent(comment.AsXML())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));

        response = client1.GetAsync($"/lbp/userComments/{profile.Username}").Result;
        SerializedCommentList userComments = response.Content.ReadAsXML<SerializedCommentList>();
        Assert.That(userComments.Items, Has.Count.EqualTo(1));
        Assert.That(userComments.Items[0].Content, Is.EqualTo(comment.Content));
        
        response = client2.PostAsync($"/lbp/deleteUserComment/{profile.Username}?commentId={userComments.Items[0].CommentId}", new ByteArrayContent(Array.Empty<byte>())).Result;
        Assert.That(response.StatusCode, Is.EqualTo(OK));

        // Make sure the comment is now gone
        response = client1.GetAsync($"/lbp/userComments/{profile.Username}").Result;
        userComments = response.Content.ReadAsXML<SerializedCommentList>();
        Assert.That(userComments.Items, Has.Count.EqualTo(0));
    }
    
    [Test]
    public void RateProfileComment()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameProfileComment comment = context.Database.PostCommentToProfile(user, user, "This is a test comment!");
        
        CommentTests.RateComment(context, user, comment, $"/lbp/rateUserComment/{user.Username}", $"/lbp/userComments/{user.Username}");
    }
}