using Refresh.Database.Models.Assets;

namespace Refresh.Core.Configuration;

public class RolePermissions
{
    public RolePermissions() {}

    public bool ReadOnlyMode { get; set; } = false;
    public ConfigAssetFlags BlockedAssetFlags { get; set; } = new(AssetFlags.Dangerous | AssetFlags.Modded);
    public TimedLevelUploadLimitProperties TimedLevelUploadLimits = new()
    {
        Enabled = false,
        TimeSpanHours = 24,
        LevelQuota = 10,
    };
}