using Refresh.Database;
using RefreshTests.GameServer.Time;

namespace RefreshTests.GameServer.GameServer;

public class TestGameDatabaseProvider : GameDatabaseProvider
{
    private readonly MockDateTimeProvider _time;
    private readonly int _databaseId = Random.Shared.Next();

    public TestGameDatabaseProvider(MockDateTimeProvider time) : base(time)
    {
        this._time = time;
    }
    
    #if POSTGRES
    public override TestGameDatabaseContext GetContext()
    {
        return new TestGameDatabaseContext(this._time, this._databaseId);
    }

    public override void Initialize()
    {
        using TestGameDatabaseContext context = this.GetContext();
        context.Database.EnsureCreated();
    }
#else
    protected override string Filename => $"realm-inmemory-{this._databaseId}";
    protected override bool InMemory => true;
    #endif
}