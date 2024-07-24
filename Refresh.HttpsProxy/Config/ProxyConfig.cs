namespace Refresh.HttpsProxy.Config;

public class ProxyConfig : Bunkum.Core.Configuration.Config
{
    public override int CurrentConfigVersion => 2;
    public override int Version { get; set; }
    
    protected override void Migrate(int oldVer, dynamic oldConfig) { }

    public string TargetServerUrl { get; set; } = "https://lbp.littlebigrefresh.com";
    public int Ps3DigestIndex { get; set; } = 0;
    public int Ps4DigestIndex { get; set; } = 0;
}