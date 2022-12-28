namespace Refresh.HttpServer.Database.Dummy;

public class DummyDatabaseProvider : IDatabaseProvider<DummyDatabaseContext>
{
    public void Initialize()
    {
        
    }

    public DummyDatabaseContext GetContext()
    {
        return new DummyDatabaseContext();
    }
    
    public void Dispose() {}
}