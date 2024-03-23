using System.Diagnostics.CodeAnalysis;
using Bunkum.Core.Configuration;
using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer.Configuration;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
[SuppressMessage("ReSharper", "RedundantDefaultMemberInitializer")]
public class GameServerConfig : Config
{
    public override int CurrentConfigVersion => 11;
    public override int Version { get; set; } = 0;

    protected override void Migrate(int oldVer, dynamic oldConfig) {}

    public string LicenseText { get; set; } = "Welcome to Refresh!";

    public AssetSafetyLevel MaximumAssetSafetyLevel { get; set; } = AssetSafetyLevel.Safe;
    public bool AllowUsersToUseIpAuthentication { get; set; } = false;
    public bool UseTicketVerification { get; set; } = true;
    public bool RegistrationEnabled { get; set; } = true;
    public string InstanceName { get; set; } = "Refresh";
    public string InstanceDescription { get; set; } = "A server running Refresh!";
    public bool MaintenanceMode { get; set; } = false;
    public bool RequireGameLoginToRegister { get; set; } = false;
    public bool TrackRequestStatistics { get; set; } = false;
    public string WebExternalUrl { get; set; } = "https://refresh.example.com";
    public bool AllowInvalidTextureGuids { get; set; } = false;
    public bool BlockAssetUploads { get; set; } = false;
    /// <summary>
    /// The amount of data the user is allowed to upload before all resource uploads get blocked, defaults to 100mb.
    /// </summary>
    public int UserFilesizeQuota { get; set; } = 100 * 1_048_576;
}