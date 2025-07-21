using NotEnoughLogs;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Workers;
using Refresh.Interfaces.Workers.Repeating;
using RefreshTests.GameServer.Logging;
using static Refresh.Database.Models.Users.GameUserRole;

namespace RefreshTests.GameServer.Tests.Workers;

public class PunishmentExpiryTests : GameServerTest
{
    [Test]
    public void BannedUsersExpire()
    {
        using TestContext context = this.GetServer();
        PunishmentExpiryJob worker = new();
        GameUser user = context.CreateUser();
        Assert.Multiple(() =>
        {
            Assert.That(context.Database.GetAllUsersWithRole(Banned).Items, Is.Empty);
            Assert.That(user.Role, Is.EqualTo(User));
        });
        
        context.Database.BanUser(user, "", DateTimeOffset.FromUnixTimeMilliseconds(1000));
        
        Assert.Multiple(() =>
        {
            Assert.That(user.Role, Is.EqualTo(Banned));
            Assert.That(context.Database.GetAllUsersWithRole(Banned).Items, Contains.Item(user));
        });
        
        worker.ExecuteJob(context.GetWorkContext());
        Assert.That(user.Role, Is.EqualTo(Banned));

        context.Time.TimestampMilliseconds = 2000;
        worker.ExecuteJob(context.GetWorkContext());
        
        context.Database.Refresh();
        user = context.Database.GetUserByObjectId(user.UserId)!;
        Assert.That(user.Role, Is.EqualTo(User));
    }

    [Test]
    public void RestrictedUsersExpire()
    {
        using TestContext context = this.GetServer();
        PunishmentExpiryJob worker = new();
        
        GameUser user = context.CreateUser();
        Assert.That(user.Role, Is.EqualTo(User));
        
        context.Database.RestrictUser(user, "", DateTimeOffset.FromUnixTimeMilliseconds(1000));
        Assert.That(user.Role, Is.EqualTo(Restricted));
        
        worker.ExecuteJob(context.GetWorkContext());
        Assert.That(user.Role, Is.EqualTo(Restricted));

        context.Time.TimestampMilliseconds = 2000;
        worker.ExecuteJob(context.GetWorkContext());
        
        context.Database.Refresh();
        user = context.Database.GetUserByObjectId(user.UserId)!;
        Assert.Multiple(() =>
        {
            Assert.That(user.Role, Is.EqualTo(User));
        });
    }
}