using NotEnoughLogs;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.Workers.Workers;
using RefreshTests.GameServer.Logging;
using static Refresh.Database.Models.Users.GameUserRole;

namespace RefreshTests.GameServer.Tests.Workers;

public class PunishmentExpiryTests : GameServerTest
{
    [Test]
    public void BannedUsersExpire()
    {
        using TestContext context = this.GetServer(false);
        using Logger logger = new(new []{ new NUnitSink() });
        
        PunishmentExpiryWorker worker = new();
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
        
        worker.DoWork(context.GetDataContext());
        Assert.Multiple(() =>
        {
            Assert.That(user.Role, Is.EqualTo(Banned));
        });

        context.Time.TimestampMilliseconds = 2000;
        worker.DoWork(context.GetDataContext());
        
        Assert.Multiple(() =>
        {
            Assert.That(user.Role, Is.EqualTo(User));
        });
    }

    [Test]
    public void RestrictedUsersExpire()
    {
        using TestContext context = this.GetServer(false);
        using Logger logger = new(new []{ new NUnitSink() });
        
        PunishmentExpiryWorker worker = new();
        
        GameUser user = context.CreateUser();
        Assert.That(user.Role, Is.EqualTo(User));
        
        context.Database.RestrictUser(user, "", DateTimeOffset.FromUnixTimeMilliseconds(1000));
        Assert.That(user.Role, Is.EqualTo(Restricted));
        
        worker.DoWork(context.GetDataContext());
        Assert.Multiple(() =>
        {
            Assert.That(user.Role, Is.EqualTo(Restricted));
        });

        context.Time.TimestampMilliseconds = 2000;
        worker.DoWork(context.GetDataContext());
        
        Assert.Multiple(() =>
        {
            Assert.That(user.Role, Is.EqualTo(User));
        });
    }
}