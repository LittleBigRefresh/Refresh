using Refresh.Database;
using RefreshTests.GameServer.Time;

#if POSTGRES
using Testcontainers.PostgreSql;
#endif

namespace RefreshTests.GameServer.GameServer;

public class TestGameDatabaseProvider : GameDatabaseProvider
{
    private readonly MockDateTimeProvider _time;
#if POSTGRES
    private readonly PostgreSqlContainer _container;
#endif

    public TestGameDatabaseProvider(MockDateTimeProvider time) : base(time)
    {
        this._time = time;
#if POSTGRES
        this._container = ContainerPool.Instance.Take();
#endif
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