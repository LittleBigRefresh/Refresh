using Refresh.Database.Models.Assets;

namespace Refresh.Core.Configuration;

public class RolePermissions
{
    public RolePermissions() {}

    public bool ReadOnlyMode { get; set; } = false;
    public ConfigAssetFlags BlockedAssetFlags { get; set; } = new(AssetFlags.Dangerous | AssetFlags.Modded);
    public EntityUploadRateLimitProperties LevelUploadRateLimit = new()
    {
        Enabled = false,
        TimeSpanHours = 24,
        UploadQuota = 10,
    };

    public EntityUploadRateLimitProperties PhotoUploadRateLimit = new()
    {
        Enabled = false,
        TimeSpanHours = 24,
        UploadQuota = 10,
    };

    public EntityUploadRateLimitProperties PlaylistUploadRateLimit = new()
    {
        Enabled = false,
        TimeSpanHours = 24,
        UploadQuota = 8,
    };

    /// <summary>
    /// The amount of data the user is allowed to upload before all resource uploads get blocked, defaults to 100mb.
    /// </summary>
    public int UserFilesizeQuota { get; set; } = 100 * 1_048_576;

    // Not configurable because "Restricted" and "Banned" already imply a user has no uploading perms.
    public static RolePermissions FromRestrictedUser => new()
    {
        ReadOnlyMode = true,
        BlockedAssetFlags = new(),
        // Not enabled because restricted may not upload UGC anyway
        LevelUploadRateLimit = new()
        {
            Enabled = false,
        },
        PhotoUploadRateLimit = new()
        {
            Enabled = false,
        },
        PlaylistUploadRateLimit = new()
        {
            Enabled = false,
        },
        UserFilesizeQuota = 0,
    };
}