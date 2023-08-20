using Bunkum.CustomHttpListener;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Storage;
using NotEnoughLogs;
using Refresh.GameServer;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;

namespace RefreshTests.GameServer.GameServer;

public class TestRefreshGameServer : RefreshGameServer
{
    public TestRefreshGameServer(BunkumHttpListener listener, Func<GameDatabaseProvider> provider) : base(listener, provider, null, new InMemoryDataStore())
    {}

    public BunkumHttpServer Server => this._server;
    public LoggerContainer<RefreshContext> Logger => this._logger;

    protected override void SetupConfiguration()
    {
        this._server.UseConfig(new GameServerConfig());
        this._server.UseConfig(new RichPresenceConfig());
        this._server.UseConfig(new IntegrationConfig());
    }

    public override void Start()
    {
        this._server.Start(1);
        // this._workerManager.Start();
    }

    protected override void SetupMiddlewares()
    {
        
    }

    protected override void SetupServices()
    {
        
    }
}