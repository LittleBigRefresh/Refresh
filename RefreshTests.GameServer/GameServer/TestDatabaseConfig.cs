using Refresh.Database.Configuration;

namespace RefreshTests.GameServer.GameServer;

public class TestDatabaseConfig : IDatabaseConfig
{
    public TestDatabaseConfig(Testcontainers.PostgreSql.PostgreSqlContainer container)
    {
        this.ConnectionString = container.GetConnectionString() + ";Include Error Detail=true";
        this.PreferConnectionStringEnvironmentVariable = false;
    }
    
    public string ConnectionString { get; }
    public bool PreferConnectionStringEnvironmentVariable { get; }
}