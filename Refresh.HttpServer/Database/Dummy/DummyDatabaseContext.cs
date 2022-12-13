namespace Refresh.HttpServer.Database.Dummy;

public class DummyDatabaseContext : IDatabaseContext
{
    public int GetDummyValue() => 69;

    public void Dispose() { }
}