using Bunkum.CustomHttpListener;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Storage;
using Refresh.GameServer;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;

namespace RefreshTests.GameServer.GameServer;

public class TestRefreshGameServer : RefreshGameServer
{
    public TestRefreshGameServer(BunkumHttpListener listener) : base(listener, new InMemoryGameDatabaseProvider(), null, new InMemoryDataStore())
    {}

    public BunkumHttpServer BunkumServer => this._server;
    public GameDatabaseProvider DatabaseProvider => this._databaseProvider;

    protected override void SetupConfiguration()
    {
        this._server.UseConfig(new GameServerConfig());
    }
}