namespace Refresh.Database.Configuration;

public interface IDatabaseConfig
{
    public string ConnectionString { get; }
    public bool PreferConnectionStringEnvironmentVariable { get; }
}