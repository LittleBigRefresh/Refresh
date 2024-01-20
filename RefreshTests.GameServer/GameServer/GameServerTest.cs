using Bunkum.Core.Storage;
using Bunkum.Protocols.Http.Direct;
using NotEnoughLogs;
using RefreshTests.GameServer.Logging;
using RefreshTests.GameServer.Time;

namespace RefreshTests.GameServer.GameServer;

[Parallelizable]
[CancelAfter(2000)]
public class GameServerTest
{
    protected static readonly Logger Logger = new(new []
    {
        new NUnitSink(),
    });
    
    // ReSharper disable once MemberCanBeMadeStatic.Global
    protected TestContext GetServer(bool startServer = true, IDataStore? dataStore = null)
    {
        DirectHttpListener listener = new(Logger);
        HttpClient client = listener.GetClient();
        MockDateTimeProvider time = new();

        TestGameDatabaseProvider provider = new(time);

        Lazy<TestRefreshGameServer> gameServer = new(() =>
        {
            TestRefreshGameServer gameServer = new(listener, () => provider, dataStore);
            gameServer.Start();

            return gameServer;
        });

        if (startServer) _ = gameServer.Value;
        else provider.Initialize();

        return new TestContext(gameServer, provider.GetContext(), client, listener, time);
    }
}