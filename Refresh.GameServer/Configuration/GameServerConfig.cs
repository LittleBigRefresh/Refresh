using System.Diagnostics.CodeAnalysis;
using Bunkum.HttpServer.Configuration;
using Refresh.GameServer.Types.Assets;

namespace Refresh.GameServer.Configuration;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
[SuppressMessage("ReSharper", "RedundantDefaultMemberInitializer")]
public class GameServerConfig : Config
{
    public override int CurrentConfigVersion => 4;
    public override int Version { get; set; } = 0;

    protected override void Migrate(int oldVer, dynamic oldConfig)
    {
        // throw new NotImplementedException();
    }

    public string AnnounceText { get; set; } = "This is the announce text. You can change the value in the configuration file!";

    public string LicenseText { get; set; } = "Welcome to Refresh!";

    public AssetSafetyLevel MaximumAssetSafetyLevel { get; set; } = AssetSafetyLevel.Safe;
    public bool AllowUsersToUseIpAuthentication { get; set; } = false;
    public bool UseTicketVerification { get; set; } = true;
    public bool RegistrationEnabled { get; set; } = true;
}