#if POSTGRES

using Microsoft.EntityFrameworkCore;
using Refresh.Common.Time;
using Refresh.Database;

namespace RefreshTests.GameServer.GameServer;

public class TestGameDatabaseContext : GameDatabaseContext
{
    private readonly int _databaseId;
    
    [Obsolete("Do not use.")]
    public TestGameDatabaseContext()
    {}

    public TestGameDatabaseContext(IDateTimeProvider time, int databaseId) : base(time)
    {
        this._databaseId = databaseId;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseInMemoryDatabase(this._databaseId.ToString());
    }
}

#endif