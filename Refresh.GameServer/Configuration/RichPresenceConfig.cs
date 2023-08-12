using Bunkum.HttpServer.Configuration;

namespace Refresh.GameServer.Configuration;

public class RichPresenceConfig : Config
{
    public override int CurrentConfigVersion => 1;
    public override int Version { get; set; } = 0;
    protected override void Migrate(int oldVer, dynamic oldConfig)
    {
        
    }

    public long ApplicationId { get; set; } = 1138956002037866648;
    public string? PodAsset { get; set; }
    public string? MoonAsset { get; set; }
    public string? RemoteMoonAsset { get; set; }
    public string? DeveloperAsset { get; set; }
    public string? DeveloperAdventureAsset { get; set; }
    public string? DlcAsset { get; set; }
    public string? FallbackAsset { get; set; }
}