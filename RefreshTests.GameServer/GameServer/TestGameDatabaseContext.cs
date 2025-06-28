using NotEnoughLogs;
using Refresh.Common.Time;
using Refresh.Database;
using Testcontainers.PostgreSql;

namespace RefreshTests.GameServer.GameServer;

public class TestGameDatabaseContext : GameDatabaseContext
{
    public TestGameDatabaseContext(Logger logger, IDateTimeProvider time, PostgreSqlContainer container) :
        base(logger, time, new TestDatabaseConfig(container))
    {}
}