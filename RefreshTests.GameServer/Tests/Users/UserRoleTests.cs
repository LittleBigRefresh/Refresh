using Refresh.Core.Configuration;
using Refresh.Core.Extensions;
using Refresh.Database.Models.Users;

namespace RefreshTests.GameServer.Tests.Users;

public class UserRoleTests : GameServerTest
{
    [Test]
    public void EnsureUsersUseCorrectRolePerms()
    {
        using TestContext context = this.GetServer();
        GameServerConfig config = context.Server.Value.GameServerConfig;
        
        config.NewUserPermissions.UserFilesizeQuota = 24;
        config.NewUserPermissions.ReadOnlyMode = false;
        
        config.NormalUserPermissions.UserFilesizeQuota = 67;
        config.NormalUserPermissions.ReadOnlyMode = true;
        
        config.TrustedUserPermissions.UserFilesizeQuota = 23456;
        config.TrustedUserPermissions.ReadOnlyMode = false;
        
        GameUser user = context.CreateUser(role: GameUserRole.NewUser);
        Assert.That(user.Role, Is.EqualTo(GameUserRole.NewUser));
        
        RolePermissions perms = user.GetRolePermissionsForUser(config);
        Assert.That(perms.UserFilesizeQuota, Is.EqualTo(24));
        Assert.That(perms.ReadOnlyMode, Is.False);
        Assert.That(user.IsWriteBlocked(config), Is.False);
        
        // Normal user
        context.Database.SetUserRole(user, GameUserRole.User);
        context.Database.Refresh();
        Assert.That(user.Role, Is.EqualTo(GameUserRole.User));
        
        perms = user.GetRolePermissionsForUser(config);
        Assert.That(perms.UserFilesizeQuota, Is.EqualTo(67));
        Assert.That(perms.ReadOnlyMode, Is.True);
        Assert.That(user.IsWriteBlocked(config), Is.True);
        
        // Trusted user
        context.Database.SetUserRole(user, GameUserRole.Trusted);
        context.Database.Refresh();
        Assert.That(user.Role, Is.EqualTo(GameUserRole.Trusted));
        
        perms = user.GetRolePermissionsForUser(config);
        Assert.That(perms.UserFilesizeQuota, Is.EqualTo(23456));
        Assert.That(perms.ReadOnlyMode, Is.False);
        Assert.That(user.IsWriteBlocked(config), Is.False);
        
        // Curator user
        context.Database.SetUserRole(user, GameUserRole.Curator);
        context.Database.Refresh();
        Assert.That(user.Role, Is.EqualTo(GameUserRole.Curator));
        
        perms = user.GetRolePermissionsForUser(config);
        Assert.That(perms.UserFilesizeQuota, Is.EqualTo(23456));
        Assert.That(perms.ReadOnlyMode, Is.False);
        Assert.That(user.IsWriteBlocked(config), Is.False);
        
        // Restricted user
        context.Database.RestrictUser(user, "lol", DateTimeOffset.MaxValue);
        context.Database.Refresh();
        Assert.That(user.Role, Is.EqualTo(GameUserRole.Restricted));
        
        perms = user.GetRolePermissionsForUser(config);
        Assert.That(perms.UserFilesizeQuota, Is.EqualTo(0));
        Assert.That(perms.ReadOnlyMode, Is.True);
        Assert.That(user.IsWriteBlocked(config), Is.True);
        
        // Banned user
        context.Database.BanUser(user, "lel", DateTimeOffset.MaxValue);
        context.Database.Refresh();
        Assert.That(user.Role, Is.EqualTo(GameUserRole.Banned));
        
        perms = user.GetRolePermissionsForUser(config);
        Assert.That(perms.UserFilesizeQuota, Is.EqualTo(0));
        Assert.That(perms.ReadOnlyMode, Is.True);
        Assert.That(user.IsWriteBlocked(config), Is.True);
    }

    [Test]
    public void EnsureNewlyCreatedUsersAreNewUsers()
    {
        // Ensure using the database method causes users to have the NewUser role
        using TestContext context = this.GetServer();
        GameUser user = context.Database.CreateUser("new", "new@new.com");
        Assert.That(user.Role, Is.EqualTo(GameUserRole.NewUser));
    }
    
    [Test]
    public void EnsureSettingRoleToBannedManuallyThrows()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        Assert.That(user.Role, Is.EqualTo(GameUserRole.NewUser));
        
        //Assert.That(() => context.Database.SetUserRole(user, GameUserRole.Restricted), Throws.TypeOf<InvalidOperationException>()); // TODO consistent behaviour
        Assert.That(() => context.Database.SetUserRole(user, GameUserRole.Banned), Throws.TypeOf<InvalidOperationException>());
    }
}