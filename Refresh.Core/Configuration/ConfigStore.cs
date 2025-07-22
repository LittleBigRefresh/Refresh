using Bunkum.Core;
using Bunkum.Core.Configuration;
using NotEnoughLogs;

namespace Refresh.Core.Configuration;

public class ConfigStore
{
    public GameServerConfig GameServer { get; }
    public DatabaseConfig DatabaseConfig { get; }
    
    public ContactInfoConfig ContactInfo { get; }
    public IntegrationConfig Integration { get; }
    public RichPresenceConfig RichPresence { get; }

    public DryArchiveConfig DryArchive { get; }

    private static readonly Lock ConfigLock = new();
    public ConfigStore(Logger logger)
    {
        lock (ConfigLock)
        {
            this.GameServer = Config.LoadFromJsonFile<GameServerConfig>("refreshGameServer.json", logger);
            this.DatabaseConfig = Config.LoadFromJsonFile<DatabaseConfig>("db.json", logger);
        
            this.ContactInfo = Config.LoadFromJsonFile<ContactInfoConfig>("contactInfo.json", logger);
            this.Integration = Config.LoadFromJsonFile<IntegrationConfig>("integrations.json", logger);
            this.RichPresence = Config.LoadFromJsonFile<RichPresenceConfig>("rpc.json", logger);
        
            this.DryArchive = Config.LoadFromJsonFile<DryArchiveConfig>("dry.json", logger);
        }
    }

    public ConfigStore()
    {
        this.GameServer = new GameServerConfig();
        this.DatabaseConfig = new DatabaseConfig();

        this.ContactInfo = new ContactInfoConfig();
        this.Integration = new IntegrationConfig();
        this.RichPresence = new RichPresenceConfig();

        this.DryArchive = new DryArchiveConfig();
    }

    public void AddToBunkum(BunkumServer server)
    {
        server.AddConfig(this.GameServer);
        server.AddConfig(this.DatabaseConfig);
        server.AddConfig(this.ContactInfo);
        server.AddConfig(this.Integration);
        server.AddConfig(this.RichPresence);
        server.AddConfig(this.DryArchive);
    }
}