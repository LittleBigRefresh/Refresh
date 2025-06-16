namespace Refresh.Database.Configuration;

public class EmptyDatabaseConfig : IDatabaseConfig
{
    #if POSTGRES
    public string ConnectionString => new Npgsql.NpgsqlConnectionStringBuilder
    {
        Database = "refresh",
        Username = "refresh",
        Password = "refresh",
        Host = "localhost",
        Port = 5432,
    }.ToString();
    #else
    public string ConnectionString => string.Empty;
    #endif

    public bool PreferConnectionStringEnvironmentVariable => false;
}