using Refresh.Database;
using RefreshTests.GameServer.Time;
using Testcontainers.PostgreSql;

namespace RefreshTests.GameServer.GameServer;

public class TestGameDatabaseProvider : GameDatabaseProvider
{
    private readonly MockDateTimeProvider _time;
    private readonly PostgreSqlContainer _container;

    public TestGameDatabaseProvider(MockDateTimeProvider time) : base(time)
    {
        this._time = time;
        this._container = ContainerPool.Instance.Take();
    }
    
    #if POSTGRES
    public override TestGameDatabaseContext GetContext()
    {
        return new TestGameDatabaseContext(this._time, this._container);
    }

    public override void Initialize()
    {
        using TestGameDatabaseContext context = this.GetContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }

    public override void Dispose()
    {
        ContainerPool.Instance.Return(this._container);
        base.Dispose();
    }
#else
    private readonly int _databaseId = Random.Shared.Next();
    protected override string Filename => $"realm-inmemory-{this._databaseId}";
    protected override bool InMemory => true;
    #endif
}