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

    /// <summary>
    /// The amount of data the user is allowed to upload before all resource uploads get blocked, defaults to 100mb.
    /// </summary>
    public int UserFilesizeQuota { get; set; } = 100 * 1_048_576;
}