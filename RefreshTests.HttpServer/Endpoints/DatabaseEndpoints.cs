using Refresh.HttpServer;
using Refresh.HttpServer.Database.Dummy;
using Refresh.HttpServer.Endpoints;
using RefreshTests.HttpServer.Database;

namespace RefreshTests.HttpServer.Endpoints;

public class DatabaseEndpoints : EndpointGroup
{
    [Endpoint("/db/null")]
    public string DatabaseNull(RequestContext context, DummyDatabaseContext? database)
    {
        return (database != null).ToString();
    }
    
    [Endpoint("/db/value")]
    public string DatabaseValue(RequestContext context, DummyDatabaseContext database)
    {
        return database.GetDummyValue().ToString();
    }
    
    [Endpoint("/db/switch")]
    public string DatabaseValue(RequestContext context, TestSwitchDatabaseContext database)
    {
        return database.GetDummyValue().ToString();
    }
}