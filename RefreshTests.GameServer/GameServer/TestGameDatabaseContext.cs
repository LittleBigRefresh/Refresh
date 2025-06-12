#if POSTGRES

using Microsoft.EntityFrameworkCore;
using Refresh.Common.Time;
using Refresh.Database;
using Testcontainers.PostgreSql;

namespace RefreshTests.GameServer.GameServer;

public class TestGameDatabaseContext : GameDatabaseContext
{
    private readonly PostgreSqlContainer _container;
    
    [Obsolete("Do not use.", true)]
    public TestGameDatabaseContext()
    {
        throw new InvalidOperationException("Do not use this constructor.");
    }

    public TestGameDatabaseContext(IDateTimeProvider time, PostgreSqlContainer container) : base(time)
    {
        this._container = container;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql(this._container.GetConnectionString() + ";Include Error Detail=true");
    }
}

#endif