using System.Diagnostics.CodeAnalysis;
using Bunkum.Core.Configuration;
using Microsoft.CSharp.RuntimeBinder;
using Refresh.GameServer.Types.Assets;
using Refresh.GameServer.Types.Roles;

namespace Refresh.GameServer.Configuration;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
[SuppressMessage("ReSharper", "RedundantDefaultMemberInitializer")]
public class GameServerConfig : Config
{
    public override int CurrentConfigVersion => 21;
    public override int Version { get; set; } = 0;
    
    protected override void Migrate(int oldVer, dynamic oldConfig)
    {
        if (oldVer < 18)
        {
            // Asset safety level was added in config version 2, so dont try to migrate if we are coming from an older version than that
            if (oldVer >= 2)
            {
                int oldSafetyLevel = (int)oldConfig.MaximumAssetSafetyLevel;
                this.BlockedAssetFlags = new ConfigAssetFlags
                {
                    Dangerous = oldSafetyLevel < 3,
                    Modded = oldSafetyLevel < 2,
                    Media = oldSafetyLevel < 1,
                };
            }

            // Asset safety level for trusted users was added in config version 12, so dont try to migrate if we are coming from a version older than that
            if (oldVer >= 12)
            {
                // There was no version bump for trusted users being added, so we just have to catch this error :/
                try
                {
                    int oldTrustedSafetyLevel = (int)oldConfig.MaximumAssetSafetyLevelForTrustedUsers;
                    this.BlockedAssetFlagsForTrustedUsers = new ConfigAssetFlags
                    {
                        Dangerous = oldTrustedSafetyLevel < 3,
                        Modded = oldTrustedSafetyLevel < 2,
                        Media = oldTrustedSafetyLevel < 1,
                    };
                }
                catch (RuntimeBinderException)
                {
                    this.BlockedAssetFlagsForTrustedUsers = this.BlockedAssetFlags;
                }
            }
        }
    }

    public string LicenseText { get; set; } = "Welcome to Refresh!";

    public ConfigAssetFlags BlockedAssetFlags { get; set; } = new(AssetFlags.Dangerous | AssetFlags.Modded);
    /// <seealso cref="GameUserRole.Trusted"/>
    public ConfigAssetFlags BlockedAssetFlagsForTrustedUsers { get; set; } = new(AssetFlags.Dangerous | AssetFlags.Modded);
    public bool AllowUsersToUseIpAuthentication { get; set; } = false;
    public bool PermitPsnLogin { get; set; } = true;
    public bool PermitRpcnLogin { get; set; } = true;
    
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
    public bool ReadOnlyMode { get; set; } = false;
    /// <seealso cref="GameUserRole.Trusted"/>
    public bool ReadonlyModeForTrustedUsers { get; set; } = false;
    /// <summary>
    /// The amount of data the user is allowed to upload before all resource uploads get blocked, defaults to 100mb.
    /// </summary>
    public int UserFilesizeQuota { get; set; } = 100 * 1_048_576;

    public TimedLevelUploadLimitProperties TimedLevelUploadLimits { get; set; } = new()
    {
        Enabled = false,
        TimeSpanHours = 24,
        LevelQuota = 10,
    };
    
    /// <summary>
    /// Whether to print the room state whenever a `FindBestRoom` match returns no results
    /// </summary>
    public bool PrintRoomStateWhenNoFoundRooms { get; set; } = true;

    public string[] Sha1DigestKeys = ["CustomServerDigest"];
    public string[] HmacDigestKeys = ["CustomServerDigest"];
}