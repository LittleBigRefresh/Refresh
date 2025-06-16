using Refresh.Database.Configuration;

namespace RefreshTests.GameServer.GameServer;

public class TestDatabaseConfig : IDatabaseConfig
{
    #if POSTGRES
    public TestDatabaseConfig(Testcontainers.PostgreSql.PostgreSqlContainer container)
    {
        this.ConnectionString = container.GetConnectionString() + ";Include Error Detail=true";
        this.PreferConnectionStringEnvironmentVariable = false;
    }
    #else
    // ReSharper disable once ConvertConstructorToMemberInitializers
    public TestDatabaseConfig()
    {
        this.ConnectionString = "";
        this.PreferConnectionStringEnvironmentVariable = false;
    }
    #endif
    
    public string ConnectionString { get; }
    public bool PreferConnectionStringEnvironmentVariable { get; }
}