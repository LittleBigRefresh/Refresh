namespace RefreshTests.GameServer.GameServer;

public class GameServerTest
{
    private TestRefreshGameServer _gameServer;
    
    public GameServerTest()
    {
        this._gameServer = new TestRefreshGameServer();
        this._gameServer.Initialize();
    }
}