using Refresh.Database.Models.Assets;

namespace RefreshTests.GameServer.Tests.Configs;

public class GameServerConfigTests : GameServerTest
{
    [Test]
    public void MigratesRolePermsFromVersion26()
    {
        TestGameServerConfig config = new()
        {
            Version = 26,
            ReadOnlyMode = true,
            ReadonlyModeForTrustedUsers = false,
            TimedLevelUploadLimits = new()
            {
                Enabled = true,
                TimeSpanHours = 67,
                LevelQuota = 2,
            },
            BlockedAssetFlags = new(AssetFlags.Dangerous | AssetFlags.Media),
            BlockedAssetFlagsForTrustedUsers = new(AssetFlags.Modded),
            UserFilesizeQuota = 141,
        };

        config.TestMigration();

        Assert.That(config.NormalUserPermissions.ReadOnlyMode, Is.True);
        Assert.That(config.TrustedUserPermissions.ReadOnlyMode, Is.False);

        Assert.That(config.NormalUserPermissions.TimedLevelUploadLimits.Enabled, Is.True);
        Assert.That(config.TrustedUserPermissions.TimedLevelUploadLimits.Enabled, Is.True);

        Assert.That(config.NormalUserPermissions.TimedLevelUploadLimits.TimeSpanHours, Is.EqualTo(67));
        Assert.That(config.TrustedUserPermissions.TimedLevelUploadLimits.TimeSpanHours, Is.EqualTo(67));

        Assert.That(config.NormalUserPermissions.TimedLevelUploadLimits.LevelQuota, Is.EqualTo(2));
        Assert.That(config.TrustedUserPermissions.TimedLevelUploadLimits.LevelQuota, Is.EqualTo(2));

        Assert.That(config.NormalUserPermissions.BlockedAssetFlags.ToAssetFlags(), Is.EqualTo(AssetFlags.Dangerous | AssetFlags.Media));
        Assert.That(config.TrustedUserPermissions.BlockedAssetFlags.ToAssetFlags(), Is.EqualTo(AssetFlags.Modded));

        Assert.That(config.NormalUserPermissions.UserFilesizeQuota, Is.EqualTo(141));
        Assert.That(config.TrustedUserPermissions.UserFilesizeQuota, Is.EqualTo(141));
    }

    [Test]
    public void MigratesRolePermsFromVersion17()
    {
        TestGameServerConfig config = new()
        {
            Version = 17,
            MaximumAssetSafetyLevel = 1,
            MaximumAssetSafetyLevelForTrustedUsers = 2,
        };

        config.TestMigration();

        Assert.That(config.NormalUserPermissions.BlockedAssetFlags.ToAssetFlags(), Is.EqualTo(AssetFlags.Dangerous | AssetFlags.Modded));
        Assert.That(config.TrustedUserPermissions.BlockedAssetFlags.ToAssetFlags(), Is.EqualTo(AssetFlags.Dangerous));
    }
}