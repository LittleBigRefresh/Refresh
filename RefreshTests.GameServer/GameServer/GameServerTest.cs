using Bunkum.CustomHttpListener.Listeners.Direct;

namespace RefreshTests.GameServer.GameServer;

[Parallelizable]
[Timeout(2000)]
public class GameServerTest
{
    // ReSharper disable once MemberCanBeMadeStatic.Global
    protected TestContext GetServer(bool start = true, bool initialize = true)
    {
        DirectHttpListener listener = new();
        HttpClient client = listener.GetClient();
        
        TestRefreshGameServer gameServer = new(listener);
        if(initialize) gameServer.Initialize();
        if(start) gameServer.Start();

        return new TestContext(gameServer, gameServer.DatabaseProvider.GetContext(), client, listener);
    }
}