using Bunkum.Core.Configuration;

namespace Refresh.GameServer.Storage;

public class DryArchiveConfig : Config
{
    public override int CurrentConfigVersion => 1;
    public override int Version { get; set; }
    
    protected override void Migrate(int oldVer, dynamic oldConfig)
    {
        
    }
    
    public bool Enabled { get; set; }
    public string Location { get; set; } = "/var/dry/";
    public bool UseFolderNames { get; set; } = true;
}