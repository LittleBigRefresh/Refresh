using Refresh.GameServer.Database;

namespace RefreshTests.GameServer.GameServer;

public class InMemoryGameDatabaseProvider : GameDatabaseProvider
{
    protected override string Filename => $"realm-inmemory-{Random.Shared.Next()}";
    protected override bool InMemory => true;
}