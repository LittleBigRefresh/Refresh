namespace Refresh.HttpServer.Database;

public interface IDatabaseProvider<out TDatabaseContext> : IDisposable where TDatabaseContext : IDatabaseContext
{
    void Initialize();

    TDatabaseContext GetContext();
}