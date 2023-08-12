using NotEnoughLogs;
using NotEnoughLogs.Loggers;
using Refresh.GameServer;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Workers;
using static Refresh.GameServer.Types.Roles.GameUserRole;

namespace RefreshTests.GameServer.Tests.Workers;

public class PunishmentExpiryTests : GameServerTest
{
    [Test]
    public void BannedUsersExpire()
    {
        using TestContext context = this.GetServer(false);
        LoggerContainer<RefreshContext> logger = new();
        logger.RegisterLogger(new ConsoleLogger());
        
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
        
        bool didWork = worker.DoWork(logger, null!, context.Database);
        Assert.Multiple(() =>
        {
            Assert.That(didWork, Is.False);
            Assert.That(user.Role, Is.EqualTo(Banned));
        });

        context.Time.TimestampMilliseconds = 2000;
        didWork = worker.DoWork(logger, null!, context.Database);
        
        Assert.Multiple(() =>
        {
            Assert.That(didWork, Is.True);
            Assert.That(user.Role, Is.EqualTo(User));
        });
    }

    [Test]
    public void RestrictedUsersExpire()
    {
        using TestContext context = this.GetServer(false);
        LoggerContainer<RefreshContext> logger = new();
        logger.RegisterLogger(new ConsoleLogger());
        
        PunishmentExpiryWorker worker = new();
        
        GameUser user = context.CreateUser();
        Assert.That(user.Role, Is.EqualTo(User));
        
        context.Database.RestrictUser(user, "", DateTimeOffset.FromUnixTimeMilliseconds(1000));
        Assert.That(user.Role, Is.EqualTo(Restricted));
        
        bool didWork = worker.DoWork(logger, null!, context.Database);
        Assert.Multiple(() =>
        {
            Assert.That(didWork, Is.False);
            Assert.That(user.Role, Is.EqualTo(Restricted));
        });

        context.Time.TimestampMilliseconds = 2000;
        didWork = worker.DoWork(logger, null!, context.Database);
        
        Assert.Multiple(() =>
        {
            Assert.That(didWork, Is.True);
            Assert.That(user.Role, Is.EqualTo(User));
        });
    }
}