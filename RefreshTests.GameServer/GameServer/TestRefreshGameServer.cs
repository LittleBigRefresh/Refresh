using Bunkum.HttpServer.Storage;
using Refresh.GameServer;
using Refresh.GameServer.Configuration;

namespace RefreshTests.GameServer.GameServer;

public class TestRefreshGameServer : RefreshGameServer
{
    public TestRefreshGameServer() : base(new InMemoryGameDatabaseProvider(), null, new InMemoryDataStore())
    {}

    protected override void SetupConfiguration()
    {
        this.Server.UseConfig(new GameServerConfig());
    }
}