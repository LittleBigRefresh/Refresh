namespace Refresh.PresenceServer.Server.Config;

public class PresenceServerConfig : Bunkum.Core.Configuration.Config
{
    public override int CurrentConfigVersion => 2;
    public override int Version { get; set; }

    /// <summary>
    /// The encryption key the presence server uses. The official key is 16 bytes located at 0x00c252ac in memory on retail LBP2.
    /// </summary>
    public byte[] Key { get; set; } = [];
    /// <summary>
    /// The host to listen on.
    /// </summary>
    public string ListenHost { get; set; } = "0.0.0.0";
    /// <summary>
    /// The port to listen on.
    /// </summary>
    public int ListenPort { get; set; } = 10072;

    /// <summary>
    /// The base host/scheme/port of the Refresh API to connect to
    /// </summary>
    public string GameServerUrl { get; set; } = "http://127.0.0.1:10061";
    /// <summary>
    /// A shared secret between the presence server and game server
    /// </summary>
    public string SharedSecret { get; set; } = "SHARED_SECRET";
    
    protected override void Migrate(int oldVer, dynamic oldConfig)
    {
        
    }
}