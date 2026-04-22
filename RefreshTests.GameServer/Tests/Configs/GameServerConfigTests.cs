using Refresh.Database.Models.Assets;
using RefreshTests.GameServer.GameServer.Configuration;

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

        Assert.That(config.NormalUserPermissions.LevelUploadRateLimit.Enabled, Is.True);
        Assert.That(config.TrustedUserPermissions.LevelUploadRateLimit.Enabled, Is.True);

        Assert.That(config.NormalUserPermissions.LevelUploadRateLimit.TimeSpanHours, Is.EqualTo(67));
        Assert.That(config.TrustedUserPermissions.LevelUploadRateLimit.TimeSpanHours, Is.EqualTo(67));

        Assert.That(config.NormalUserPermissions.LevelUploadRateLimit.EntityQuota, Is.EqualTo(2));
        Assert.That(config.TrustedUserPermissions.LevelUploadRateLimit.EntityQuota, Is.EqualTo(2));

        Assert.That(config.NormalUserPermissions.BlockedAssetFlags.ToAssetFlags(), Is.EqualTo(AssetFlags.Dangerous | AssetFlags.Media));
        Assert.That(config.TrustedUserPermissions.BlockedAssetFlags.ToAssetFlags(), Is.EqualTo(AssetFlags.Modded));

        Assert.That(config.NormalUserPermissions.UserFilesizeQuota, Is.EqualTo(141));
        Assert.That(config.TrustedUserPermissions.UserFilesizeQuota, Is.EqualTo(141));
    }

    [Test]
    public void MigratesEntityUploadRateLimitsFromVersion27()
    {
        TestGameServerConfig config = new()
        {
            Version = 27,
            NormalUserPermissions = new TestRolePermissions()
            {
                TimedLevelUploadLimits = new()
                {
                    Enabled = true,
                    TimeSpanHours = 1234567,
                    LevelQuota = 852094,
                },
            },
            TrustedUserPermissions = new TestRolePermissions()
            {
                TimedLevelUploadLimits = new()
                {
                    Enabled = false,
                    TimeSpanHours = 230,
                    LevelQuota = 7122036,
                },
            },
        };

        config.TestMigration();

        Assert.That(config.NormalUserPermissions.LevelUploadRateLimit.Enabled, Is.True);
        Assert.That(config.NormalUserPermissions.LevelUploadRateLimit.TimeSpanHours, Is.EqualTo(1234567));
        Assert.That(config.NormalUserPermissions.LevelUploadRateLimit.EntityQuota, Is.EqualTo(852094));

        Assert.That(config.TrustedUserPermissions.LevelUploadRateLimit.Enabled, Is.False);
        Assert.That(config.TrustedUserPermissions.LevelUploadRateLimit.TimeSpanHours, Is.EqualTo(230));
        Assert.That(config.TrustedUserPermissions.LevelUploadRateLimit.EntityQuota, Is.EqualTo(7122036));
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