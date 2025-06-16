using Bunkum.Core.Configuration;
using Refresh.Database;
using Refresh.Database.Configuration;

namespace Refresh.Core.Configuration;

public class DatabaseConfig : Config, IDatabaseConfig
{
    public override int CurrentConfigVersion => 1;
    public override int Version { get; set; }
    
    protected override void Migrate(int oldVer, dynamic oldConfig)
    {
        
    }

    public string ConnectionString { get; set; } = "Database=refresh;Username=refresh;Password=refresh;Host=localhost;Port=5432";
    public bool PreferConnectionStringEnvironmentVariable { get; set; } = false;
}