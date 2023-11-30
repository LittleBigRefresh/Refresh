using Microsoft.EntityFrameworkCore;

namespace Bunkum.EntityFrameworkDatabase;

public abstract class BunkumDbContext(Action<DbContextOptionsBuilder> configureAction) : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        configureAction.Invoke(options);
    }
}