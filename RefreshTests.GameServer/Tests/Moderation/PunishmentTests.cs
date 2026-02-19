using Refresh.Database.Models.Users;

namespace RefreshTests.GameServer.Tests.Moderation;

public class PunishmentTests : GameServerTest
{
    [Test]
    public void CanPunishWithOffsetExpiryDate()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        GameUser mod = context.CreateUser();
        DateTime now = context.Time.Now.DateTime;

        // Restrict
        Assert.That(() =>
        {
            context.Database.RestrictUser(user, "too many skill issues", new(now.AddHours(4), new(2, 0, 0)));
        }, Throws.Nothing);
        
        context.Database.Refresh();
        GameUser? restrictedUser = context.Database.GetUserByObjectId(user.UserId);
        Assert.That(restrictedUser, Is.Not.Null);
        Assert.That(restrictedUser!.Role, Is.EqualTo(GameUserRole.Restricted));
        Assert.That(restrictedUser!.BanExpiryDate, Is.Not.Null);
        Assert.That(restrictedUser!.BanExpiryDate!.Value.DateTime.Equals(now.AddHours(2)), Is.True);

        // Ban
        Assert.That(() =>
        {
            // Passing user here, for some reason, makes the ban not get written but also doesn't cause a throw
            context.Database.BanUser(restrictedUser, "even more skill issues", new(now.AddHours(8), new(4, 0, 0)));
        }, Throws.Nothing);
        
        context.Database.Refresh();
        GameUser? bannedUser = context.Database.GetUserByObjectId(restrictedUser.UserId);
        Assert.That(bannedUser, Is.Not.Null);
        Assert.That(bannedUser!.Role, Is.EqualTo(GameUserRole.Banned));
        Assert.That(bannedUser!.BanExpiryDate, Is.Not.Null);
        Assert.That(bannedUser!.BanExpiryDate!.Value.DateTime.Equals(now.AddHours(4)), Is.True);
    }
}