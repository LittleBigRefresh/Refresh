using Bunkum.Core.Database;
using Microsoft.EntityFrameworkCore;

namespace Bunkum.EntityFrameworkDatabase;

public class EntityFrameworkDatabaseProvider<TDatabaseContext> : IDatabaseProvider<TDatabaseContext>
    where TDatabaseContext : BunkumDbContext, IDatabaseContext
{
    private readonly Action<DbContextOptionsBuilder> _configureAction;
    
    public EntityFrameworkDatabaseProvider(Action<DbContextOptionsBuilder> configureAction)
    {
        this._configureAction = configureAction;
    }
    
    public void Initialize()
    {
        // TODO?
    }

    public void Warmup()
    {
        // TODO
    }

    public TDatabaseContext GetContext()
    {
        TDatabaseContext context = (TDatabaseContext)Activator.CreateInstance(typeof(TDatabaseContext), this._configureAction)!;
        return context;
    }
    
    public void Dispose()
    {
        
    }
}