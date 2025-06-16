#if POSTGRES

using Refresh.Common.Time;
using Refresh.Database;
using Testcontainers.PostgreSql;

namespace RefreshTests.GameServer.GameServer;

public class TestGameDatabaseContext : GameDatabaseContext
{
    public TestGameDatabaseContext(IDateTimeProvider time, PostgreSqlContainer container) :
        base(time, new TestDatabaseConfig(container))
    {}
}

#endif