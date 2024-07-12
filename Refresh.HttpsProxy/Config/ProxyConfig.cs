namespace Refresh.HttpsProxy.Config;

public class ProxyConfig : Bunkum.Core.Configuration.Config
{
    public override int CurrentConfigVersion => 1;
    public override int Version { get; set; }
    
    protected override void Migrate(int oldVer, dynamic oldConfig) { }

    public string TargetServerUrl { get; set; } = "https://lbp.littlebigrefresh.com";
    public string Ps3Digest { get; set; } = "CustomServerDigest";
    public string Ps4Digest { get; set; } = "CustomServerDigest";
}