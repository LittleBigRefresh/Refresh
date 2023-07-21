using Bunkum.CustomHttpListener.Listeners.Direct;
using Bunkum.HttpServer;
using NotEnoughLogs;
using NotEnoughLogs.Loggers;

namespace RefreshTests.GameServer.GameServer;

[Timeout(2000)]
public class GameServerTest
{
    protected static readonly LoggerContainer<BunkumContext> Logger = new();

    static GameServerTest()
    {
        Logger.RegisterLogger(new ConsoleLogger());
    }
    
    // ReSharper disable once MemberCanBeMadeStatic.Global
    protected TestContext GetServer(bool startServer = true)
    {
        DirectHttpListener listener = new();
        HttpClient client = listener.GetClient();

        InMemoryGameDatabaseProvider provider = new();

        Lazy<TestRefreshGameServer> gameServer = new(() =>
        {
            TestRefreshGameServer gameServer = new(listener, () => provider);
            gameServer.Start();

            return gameServer;
        });

        if (startServer) _ = gameServer.Value;

        return new TestContext(gameServer, provider.GetContext(), client, listener);
    }
}