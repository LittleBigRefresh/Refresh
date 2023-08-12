using Refresh.GameServer.Types.Comments;
using Refresh.GameServer.Types.UserData;

namespace RefreshTests.GameServer.Tests.Relations;

public class CommentPublishTests : GameServerTest
{
    [Test]
    public void CanCreateCommentOnProfile()
    {
        using TestContext context = this.GetServer(false);
        GameUser profile = context.CreateUser();
        GameUser commenter = context.CreateUser();
        
        GameComment comment = context.Database.PostCommentToProfile(profile, commenter, "Hi!");
        
        Assert.That(context.Database.GetProfileComments(profile, 1, 0).First(), Is.EqualTo(comment));
    }
}