namespace Refresh.Database.Configuration;

public class EmptyDatabaseConfig : IDatabaseConfig
{
    public string ConnectionString => new Npgsql.NpgsqlConnectionStringBuilder
    {
        Database = "refresh",
        Username = "refresh",
        Password = "refresh",
        Host = "localhost",
        Port = 5432,
    }.ToString();

    public bool PreferConnectionStringEnvironmentVariable => false;
}