using NotEnoughLogs;
using Refresh.Database;
using RefreshTests.GameServer.Time;
using Testcontainers.PostgreSql;

namespace RefreshTests.GameServer.GameServer;

public class TestGameDatabaseProvider : GameDatabaseProvider
{
    private readonly MockDateTimeProvider _time;
    private readonly PostgreSqlContainer _container;

    public TestGameDatabaseProvider(Logger logger, MockDateTimeProvider time) : base(logger, time)
    {
        this._time = time;
        this._container = ContainerPool.Instance.Take();
    }
    
    public override TestGameDatabaseContext GetContext()
    {
        return new TestGameDatabaseContext(this.Logger, this._time, this._container);
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
}