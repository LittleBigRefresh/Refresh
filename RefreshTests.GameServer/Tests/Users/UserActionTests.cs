using Refresh.Database.Models.Users;

namespace RefreshTests.GameServer.Tests.Users;

public class UserActionTests : GameServerTest
{
    [Test]
    public void RenamesUser()
    {
        using TestContext context = this.GetServer(false);
        GameUser? user = context.CreateUser("gamer1");
        
        Assert.That(user.Username, Is.EqualTo("gamer1"));
        
        context.Database.RenameUser(user, "gamer2");
        user = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(user, Is.Not.Null);
        
        Assert.That(user.Username, Is.EqualTo("gamer2"));
    }
}