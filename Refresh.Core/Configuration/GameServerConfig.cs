using System.Diagnostics.CodeAnalysis;
using Bunkum.Core.Configuration;
using Microsoft.CSharp.RuntimeBinder;

namespace Refresh.Core.Configuration;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
[SuppressMessage("ReSharper", "RedundantDefaultMemberInitializer")]
public class GameServerConfig : Config
{
    public override int CurrentConfigVersion => 29;
    public override int Version { get; set; } = 0;
    
    protected override void Migrate(int oldVer, dynamic oldConfig)
    {
        // In version 27, various (mostly already role-specific) perms, like blocked assets and read-only mode, were moved to dedicated child objects,
        // to more cleanly split the perms between certain roles, and to make their enforcement easier.
        if (oldVer < 27)
        {
            this.NewUserPermissions = new();
            this.NormalUserPermissions = new();
            this.TrustedUserPermissions = new();

            // filesize quota limit was added during version 11, but the version wasn't bumped, so catch error to be safe
            // Migrate filesize quota
            if (oldVer >= 11)
            {
                try
                {
                    this.NewUserPermissions.UserFilesizeQuota = (int)oldConfig.UserFilesizeQuota;
                    this.NormalUserPermissions.UserFilesizeQuota = (int)oldConfig.UserFilesizeQuota;
                    this.TrustedUserPermissions.UserFilesizeQuota = (int)oldConfig.UserFilesizeQuota;
                }
                catch (RuntimeBinderException)
                {
                    // do nothing
                }
            }

            // Migrate asset flags/safety level
            if (oldVer >= 18)
            {
                this.NewUserPermissions.BlockedAssetFlags.Dangerous = (bool)oldConfig.BlockedAssetFlags.Dangerous;
                this.NewUserPermissions.BlockedAssetFlags.Media = (bool)oldConfig.BlockedAssetFlags.Media;
                this.NewUserPermissions.BlockedAssetFlags.Modded = (bool)oldConfig.BlockedAssetFlags.Modded;
                
                this.NormalUserPermissions.BlockedAssetFlags.Dangerous = (bool)oldConfig.BlockedAssetFlags.Dangerous;
                this.NormalUserPermissions.BlockedAssetFlags.Media = (bool)oldConfig.BlockedAssetFlags.Media;
                this.NormalUserPermissions.BlockedAssetFlags.Modded = (bool)oldConfig.BlockedAssetFlags.Modded;

                this.TrustedUserPermissions.BlockedAssetFlags.Dangerous = (bool)oldConfig.BlockedAssetFlagsForTrustedUsers.Dangerous;
                this.TrustedUserPermissions.BlockedAssetFlags.Media = (bool)oldConfig.BlockedAssetFlagsForTrustedUsers.Media;
                this.TrustedUserPermissions.BlockedAssetFlags.Modded = (bool)oldConfig.BlockedAssetFlagsForTrustedUsers.Modded;
            }
            else
            {
                // Asset safety level was added in config version 2, so dont try to migrate if we are coming from an older version than that
                if (oldVer >= 2)
                {
                    int oldSafetyLevel = (int)oldConfig.MaximumAssetSafetyLevel;
                    ConfigAssetFlags fromSafetyLevel = new ConfigAssetFlags
                    {
                        Dangerous = oldSafetyLevel < 3,
                        Modded = oldSafetyLevel < 2,
                        Media = oldSafetyLevel < 1,
                    };
                    this.NormalUserPermissions.BlockedAssetFlags = fromSafetyLevel;
                    this.NewUserPermissions.BlockedAssetFlags = fromSafetyLevel;
                }

                // Asset safety level for trusted users was added in config version 12, so dont try to migrate if we are coming from a version older than that
                if (oldVer >= 12)
                {
                    // There was no version bump for trusted users being added, so we just have to catch this error :/
                    try
                    {
                        int oldTrustedSafetyLevel = (int)oldConfig.MaximumAssetSafetyLevelForTrustedUsers;
                        this.TrustedUserPermissions.BlockedAssetFlags = new ConfigAssetFlags
                        {
                            Dangerous = oldTrustedSafetyLevel < 3,
                            Modded = oldTrustedSafetyLevel < 2,
                            Media = oldTrustedSafetyLevel < 1,
                        };
                    }
                    catch (RuntimeBinderException)
                    {
                        this.TrustedUserPermissions.BlockedAssetFlags = this.NormalUserPermissions.BlockedAssetFlags;
                    }
                }
            }

            // Timed level upload limits were added in version 19.
            // Migrate level limits
            if (oldVer >= 19)
            {
                this.NewUserPermissions.LevelUploadRateLimit.Enabled = (bool)oldConfig.TimedLevelUploadLimits.Enabled;
                this.NewUserPermissions.LevelUploadRateLimit.TimeSpanHours = (int)oldConfig.TimedLevelUploadLimits.TimeSpanHours;
                this.NewUserPermissions.LevelUploadRateLimit.UploadQuota = (int)oldConfig.TimedLevelUploadLimits.LevelQuota;
                
                this.NormalUserPermissions.LevelUploadRateLimit.Enabled = (bool)oldConfig.TimedLevelUploadLimits.Enabled;
                this.NormalUserPermissions.LevelUploadRateLimit.TimeSpanHours = (int)oldConfig.TimedLevelUploadLimits.TimeSpanHours;
                this.NormalUserPermissions.LevelUploadRateLimit.UploadQuota = (int)oldConfig.TimedLevelUploadLimits.LevelQuota;

                this.TrustedUserPermissions.LevelUploadRateLimit.Enabled = (bool)oldConfig.TimedLevelUploadLimits.Enabled;
                this.TrustedUserPermissions.LevelUploadRateLimit.TimeSpanHours = (int)oldConfig.TimedLevelUploadLimits.TimeSpanHours;
                this.TrustedUserPermissions.LevelUploadRateLimit.UploadQuota = (int)oldConfig.TimedLevelUploadLimits.LevelQuota;
            }

            // Read-only mode was added for both normal and trusted users in version 20.
            if (oldVer >= 20)
            {
                this.NewUserPermissions.ReadOnlyMode = (bool)oldConfig.ReadOnlyMode;
                this.NormalUserPermissions.ReadOnlyMode = (bool)oldConfig.ReadOnlyMode;
                this.TrustedUserPermissions.ReadOnlyMode = (bool)oldConfig.ReadonlyModeForTrustedUsers;
            }
        }

        // In version 28, PhotoUploadRateLimit and PlaylistUploadRateLimit were added to RolePermissions
        // and various attributes related to level rate-limiting were renamed
        else if (oldVer == 27)
        {
            this.NormalUserPermissions.LevelUploadRateLimit.Enabled = (bool)oldConfig.NormalUserPermissions.TimedLevelUploadLimits.Enabled;
            this.NormalUserPermissions.LevelUploadRateLimit.TimeSpanHours = (int)oldConfig.NormalUserPermissions.TimedLevelUploadLimits.TimeSpanHours;
            this.NormalUserPermissions.LevelUploadRateLimit.UploadQuota = (int)oldConfig.NormalUserPermissions.TimedLevelUploadLimits.LevelQuota;
            
            this.NormalUserPermissions.LevelUploadRateLimit.Enabled = (bool)oldConfig.NormalUserPermissions.TimedLevelUploadLimits.Enabled;
            this.NormalUserPermissions.LevelUploadRateLimit.TimeSpanHours = (int)oldConfig.NormalUserPermissions.TimedLevelUploadLimits.TimeSpanHours;
            this.NormalUserPermissions.LevelUploadRateLimit.UploadQuota = (int)oldConfig.NormalUserPermissions.TimedLevelUploadLimits.LevelQuota;

            this.TrustedUserPermissions.LevelUploadRateLimit.Enabled = (bool)oldConfig.TrustedUserPermissions.TimedLevelUploadLimits.Enabled;
            this.TrustedUserPermissions.LevelUploadRateLimit.TimeSpanHours = (int)oldConfig.TrustedUserPermissions.TimedLevelUploadLimits.TimeSpanHours;
            this.TrustedUserPermissions.LevelUploadRateLimit.UploadQuota = (int)oldConfig.TrustedUserPermissions.TimedLevelUploadLimits.LevelQuota;
        }
        
        // In version 29, the NewUser role and its related config options
        // (new user role perms and HoursUntilNewAccountNoLongerNew) were added
        else if (oldVer < 29)
        {
            this.NewUserPermissions = oldConfig.NormalUserPermissions;
        }
    }

    public string LicenseText { get; set; } = "Welcome to Refresh!";

    /// <summary>
    /// Role-specific permissions for new users.
    /// </summary>
    public RolePermissions NewUserPermissions = new();
    /// <summary>
    /// Role-specific permissions for normal, not-new users.
    /// </summary>
    public RolePermissions NormalUserPermissions = new();
    /// <summary>
    /// Role-specific permissions for trusted users and above.
    /// </summary>
    public RolePermissions TrustedUserPermissions = new();

    /// <summary>
    /// How long we should wait (in hours) until we should use NewUserJob to set a new user's role from NewUser to User,
    /// effectively marking them as no longer new.
    /// Once their account hits this age, we will start applying NormalUserPermissions instead of NewUserPermissions
    /// as their role-perms.
    /// </summary>
    public int HoursUntilNewAccountNoLongerNew { get; set; } = 24 * 7; // TODO should we think of a better name?
    
    public bool AllowUsersToUseIpAuthentication { get; set; } = false;
    public bool PermitPsnLogin { get; set; } = true;
    public bool PermitRpcnLogin { get; set; } = true;
    public bool PermitWebLogin { get; set; } = true;
    /// <summary>
    /// Secondary safety switch incase the PSN and RPCN toggles somehow fail.
    /// </summary>
    public bool PermitAllLogins { get; set; } = true;

    /// <summary>
    /// Should all game logins be required to use Patchwork?
    /// </summary>
    public bool EnforcePatchwork { get; set; } = true;

    /// <summary>
    /// The minimum required major version of Patchwork on the client to be able to connect.
    /// </summary>
    public int RequiredPatchworkMajorVersion { get; set; } = 1;
    
    /// <summary>
    /// The minimum required minor version of Patchwork on the client to be able to connect.
    /// </summary>
    public int RequiredPatchworkMinorVersion { get; set; } = 0;
    
    public bool UseTicketVerification { get; set; } = true;
    public bool RegistrationEnabled { get; set; } = true;
    public string InstanceName { get; set; } = "Refresh";
    public string InstanceDescription { get; set; } = "A server running Refresh!";
    public bool MaintenanceMode { get; set; } = false;
    public bool RequireGameLoginToRegister { get; set; } = false;
    /// <summary>
    /// Whether to use deflate compression for responses.
    /// If this is disabled, large enough responses will cause LBP to overflow its read buffer and eventually corrupt its own memory to the point of crashing.
    /// </summary>
    public bool UseDeflateCompression { get; set; } = true;
    public string WebExternalUrl { get; set; } = "https://refresh.example.com";
    /// <summary>
    /// The base URL that LBP3 uses to grab config files like `network_settings.nws`.
    /// </summary>
    public string GameConfigStorageUrl { get; set; } = "https://refresh.example.com/lbp";
    public bool AllowInvalidTextureGuids { get; set; } = false;
    
    /// <summary>
    /// Whether to print the room state whenever a `FindBestRoom` match returns no results
    /// </summary>
    public bool PrintRoomStateWhenNoFoundRooms { get; set; } = true;

    /// <summary>
    /// Whether to unconditionally print data like token, token owner, remote IP, request URI etc during authentication outside of exceptions
    /// </summary>
    public bool PrintAuthenticationData { get; set; } = false;

    public string[] Sha1DigestKeys = ["CustomServerDigest"];
    public string[] HmacDigestKeys = ["CustomServerDigest"];

    public bool PermitShowingOnlineUsers { get; set; } = true;
    
    public bool EnableDiveIn { get; set; } = true;
}