using Refresh.GameServer.Types.UserData;

namespace RefreshTests.GameServer.Tests.Relations;

public class MutualTests : GameServerTest
{
    [Test]
    public void CanGetMutualUsers()
    {
        using TestContext context = this.GetServer(false);
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        
        Assert.Multiple(() =>
        {
            Assert.That(context.Database.AreUsersMutual(user1, user2), Is.False);
            Assert.That(context.Database.AreUsersMutual(user2, user1), Is.False);
        });

        context.Database.FavouriteUser(user1, user2);
        
        Assert.Multiple(() =>
        {
            Assert.That(context.Database.AreUsersMutual(user1, user2), Is.False);
            Assert.That(context.Database.AreUsersMutual(user2, user1), Is.False);
        });
        
        context.Database.FavouriteUser(user2, user1);
        
        Assert.Multiple(() =>
        {
            Assert.That(context.Database.AreUsersMutual(user1, user2), Is.True);
            Assert.That(context.Database.AreUsersMutual(user2, user1), Is.True);
        });
    }

    [Test]
    public void CanGetListOfMutuals()
    {
        using TestContext context = this.GetServer(false);
        GameUser user1 = context.CreateUser();
        GameUser user2 = context.CreateUser();
        GameUser user3 = context.CreateUser();
        GameUser user4 = context.CreateUser();
        
        // horrendous.
        context.Database.FavouriteUser(user1, user2);
        context.Database.FavouriteUser(user2, user1);
        
        context.Database.FavouriteUser(user1, user3);
        context.Database.FavouriteUser(user3, user1);
        
        context.Database.FavouriteUser(user1, user4);
        context.Database.FavouriteUser(user4, user1);

        IEnumerable<GameUser> mutuals = context.Database.GetUsersMutuals(user1).ToList();

        Assert.That(mutuals, Does.Contain(user2));
        Assert.That(mutuals, Does.Contain(user3));
        Assert.That(mutuals, Does.Contain(user4));
    }
}