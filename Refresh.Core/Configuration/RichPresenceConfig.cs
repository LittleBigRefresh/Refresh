using Bunkum.Core.Configuration;

namespace Refresh.GameServer.Configuration;

public class RichPresenceConfig : Config
{
    public override int CurrentConfigVersion => 2;
    public override int Version { get; set; }
    protected override void Migrate(int oldVer, dynamic oldConfig)
    {
        
    }
    
    public long ApplicationId { get; set; } = 1138956002037866648;
    public bool UseApplicationAssets { get; set; } = true;
    public string? PodAsset { get; set; }
    public string? MoonAsset { get; set; }
    public string? RemoteMoonAsset { get; set; }
    public string? DeveloperAsset { get; set; }
    public string? DeveloperAdventureAsset { get; set; }
    public string? DlcAsset { get; set; }
    public string? FallbackAsset { get; set; }
}