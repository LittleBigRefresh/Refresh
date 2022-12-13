using Refresh.HttpServer.Database;
using Refresh.HttpServer.Database.Dummy;

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
}