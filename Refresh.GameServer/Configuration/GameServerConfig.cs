using System.Diagnostics.CodeAnalysis;
using Bunkum.Core.Configuration;
using Refresh.GameServer.Types.Assets;
using Refresh.GameServer.Types.Roles;

namespace Refresh.GameServer.Configuration;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
[SuppressMessage("ReSharper", "RedundantDefaultMemberInitializer")]
public class GameServerConfig : Config
{
    public override int CurrentConfigVersion => 14;
    public override int Version { get; set; } = 0;

    protected override void Migrate(int oldVer, dynamic oldConfig) {}

    public string LicenseText { get; set; } = "Welcome to Refresh!";

    public AssetSafetyLevel MaximumAssetSafetyLevel { get; set; } = AssetSafetyLevel.SafeMedia;
    /// <seealso cref="GameUserRole.Trusted"/>
    public AssetSafetyLevel MaximumAssetSafetyLevelForTrustedUsers { get; set; } = AssetSafetyLevel.SafeMedia;
    public bool AllowUsersToUseIpAuthentication { get; set; } = false;
    public bool UseTicketVerification { get; set; } = true;
    public bool RegistrationEnabled { get; set; } = true;
    public string InstanceName { get; set; } = "Refresh";
    public string InstanceDescription { get; set; } = "A server running Refresh!";
    public bool MaintenanceMode { get; set; } = false;
    public bool RequireGameLoginToRegister { get; set; } = false;
    public bool TrackRequestStatistics { get; set; } = false;
    /// <summary>
    /// Whether to use deflate compression for responses.
    /// If this is disabled, large enough responses will cause LBP to overflow its read buffer and eventually corrupt its own memory to the point of crashing.
    /// </summary>
    public bool UseDeflateCompression { get; set; } = true;
    public string WebExternalUrl { get; set; } = "https://refresh.example.com";
    /// <summary>
    /// The base URL that LBP3 uses to grab config files like `network_settings.nws`.
    /// URL must point to a HTTPS capable server with TLSv1.2 connectivity, the game will automatically correct HTTP to HTTPS. 
    /// </summary>
    public string GameConfigStorageUrl { get; set; } = "https://refresh.example.com/lbp";
    public bool AllowInvalidTextureGuids { get; set; } = false;
    public bool BlockAssetUploads { get; set; } = false;
    /// <seealso cref="GameUserRole.Trusted"/>
    public bool BlockAssetUploadsForTrustedUsers { get; set; } = false;
    /// <summary>
    /// The amount of data the user is allowed to upload before all resource uploads get blocked, defaults to 100mb.
    /// </summary>
    public int UserFilesizeQuota { get; set; } = 100 * 1_048_576;
}