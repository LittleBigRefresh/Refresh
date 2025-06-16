using Refresh.Database.Configuration;
using Testcontainers.PostgreSql;

namespace RefreshTests.GameServer.GameServer;

public class TestDatabaseConfig : IDatabaseConfig
{
    public TestDatabaseConfig(PostgreSqlContainer container)
    {
        this.ConnectionString = container.GetConnectionString() + ";Include Error Detail=true";
        this.PreferConnectionStringEnvironmentVariable = false;
    }
    
    public string ConnectionString { get; }
    public bool PreferConnectionStringEnvironmentVariable { get; }
}