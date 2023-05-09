using Refresh.GameServer.Database;

namespace RefreshTests.GameServer.GameServer;

public class InMemoryGameDatabaseProvider : GameDatabaseProvider
{
    private readonly int _databaseId = Random.Shared.Next();
    protected override string Filename => $"realm-inmemory-{this._databaseId}";
    protected override bool InMemory => true;
}