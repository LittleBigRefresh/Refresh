using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Users;

namespace RefreshTests.GameServer.Tests.Relations;

public class CommentPublishTests : GameServerTest
{
    [Test]
    public void CanCreateCommentOnProfile()
    {
        using TestContext context = this.GetServer(false);
        GameUser profile = context.CreateUser();
        GameUser commenter = context.CreateUser();
        
        GameProfileComment comment = context.Database.PostCommentToProfile(profile, commenter, "Hi!");
        
        Assert.That(context.Database.GetProfileComments(profile, 1, 0).First(), Is.EqualTo(comment));
    }
}