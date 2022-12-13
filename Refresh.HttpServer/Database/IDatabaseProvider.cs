namespace Refresh.HttpServer.Database;

public interface IDatabaseProvider<out TDatabaseContext> where TDatabaseContext : IDatabaseContext
{
    void Initialize();

    TDatabaseContext GetContext();
}