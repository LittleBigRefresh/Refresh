using Refresh.HttpServer.Database;

namespace RefreshTests.HttpServer.Database;

public class TestSwitchDatabaseContext : IDatabaseContext
{
    public int GetDummyValue() => 420;

    public void Dispose() { }
}