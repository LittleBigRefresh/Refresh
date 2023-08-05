using Bunkum.CustomHttpListener;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Storage;
using Refresh.GameServer;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;

namespace RefreshTests.GameServer.GameServer;

public class TestRefreshGameServer : RefreshGameServer
{
    public TestRefreshGameServer(BunkumHttpListener listener, Func<GameDatabaseProvider> provider) : base(listener, provider, null, new InMemoryDataStore())
    {}

    protected override void SetupConfiguration()
    {
        this._server.UseConfig(new GameServerConfig());
    }

    public override void Start()
    {
        this._server.Start(1);
    }

    protected override void SetupMiddlewares()
    {
        
    }

    protected override void SetupServices()
    {
        
    }
}