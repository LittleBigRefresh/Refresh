using Refresh.HttpServer.Database;

namespace RefreshTests.HttpServer.Database;

public class TestSwitchDatabaseProvider : IDatabaseProvider<TestSwitchDatabaseContext>
{
    public void Initialize()
    {
        
    }

    public TestSwitchDatabaseContext GetContext()
    {
        return new TestSwitchDatabaseContext();
    }
    
    public void Dispose() {}
}