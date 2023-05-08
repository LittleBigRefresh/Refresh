using Refresh.GameServer.Database;

namespace RefreshTests.GameServer.GameServer;

public class InMemoryGameDatabaseProvider : GameDatabaseProvider
{
    protected override string Filename => Path.Combine(Path.GetTempPath(), $"realm-inmemory-{Environment.CurrentManagedThreadId}");
    protected override bool InMemory => Environment.GetEnvironmentVariable("CI") == null;
}