using System.Diagnostics.CodeAnalysis;
using Bunkum.Core.Configuration;
using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer.Configuration;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
[SuppressMessage("ReSharper", "RedundantDefaultMemberInitializer")]
public class GameServerConfig : Config
{
    public override int CurrentConfigVersion => 10;
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
    public bool EnableUnknownGuids { get; set; } = false;
}