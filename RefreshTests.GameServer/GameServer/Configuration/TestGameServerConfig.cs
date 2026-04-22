using Refresh.Core.Configuration;
using Refresh.Database.Models.Assets;

namespace RefreshTests.GameServer.GameServer.Configuration;

public class TestGameServerConfig : GameServerConfig
{
    public void TestMigration()
    {
        this.Migrate(this.Version, this);
    }

    // Various attributes to migrate from
    public ConfigAssetFlags BlockedAssetFlags { get; set; } = new(AssetFlags.Dangerous | AssetFlags.Modded);
    public ConfigAssetFlags BlockedAssetFlagsForTrustedUsers { get; set; } = new(AssetFlags.Dangerous | AssetFlags.Modded);
    public bool ReadOnlyMode { get; set; } = false;
    public bool ReadonlyModeForTrustedUsers { get; set; } = false;
    public TestEntityUploadRateLimitProperties TimedLevelUploadLimits { get; set; } = new()
    {
        Enabled = false,
        TimeSpanHours = 24,
        LevelQuota = 10,
    };

    public int MaximumAssetSafetyLevel { get; set; } = 0;
    public int MaximumAssetSafetyLevelForTrustedUsers { get; set; } = 0;
    public int UserFilesizeQuota { get; set; } = 141;
}