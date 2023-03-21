using System.Diagnostics.CodeAnalysis;
using Bunkum.HttpServer.Configuration;

namespace Refresh.GameServer.Configuration;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public class GameServerConfig : Config
{
    public override int CurrentConfigVersion => 1;
    public override int Version { get; set; }

    protected override void Migrate(int oldVer, dynamic oldConfig)
    {
        // throw new NotImplementedException();
    }
    
    public string AnnounceText { get; set; } = "This is the announce text. You can change the value in the configuration file!";

    public string LicenseText { get; set; } = "Welcome to Refresh!";
}