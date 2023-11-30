using Refresh.GameServer.Database;
using Refresh.GameServer.Database.Realm;
using RefreshTests.GameServer.Time;

namespace RefreshTests.GameServer.GameServer;

public class TestRealmGameDatabaseProvider : RealmGameDatabaseProvider
{
    public TestRealmGameDatabaseProvider(MockDateTimeProvider time) : base(time)
    {}

    private readonly int _databaseId = Random.Shared.Next();
    protected override string Filename => $"realm-inmemory-{this._databaseId}";
    protected override bool InMemory => true;
}