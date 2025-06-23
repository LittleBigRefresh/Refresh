using Refresh.Common.Time;
using Refresh.Database.Configuration;

namespace Refresh.Database;

public class GameDatabaseProvider : 
    IDatabaseProvider<GameDatabaseContext>
{
    private readonly IDateTimeProvider _time;
    private readonly IDatabaseConfig _dbConfig;

    public GameDatabaseProvider(IDatabaseConfig dbConfig)
    {
        this._time = new SystemDateTimeProvider();
        this._dbConfig = dbConfig;
    }

    protected GameDatabaseProvider(IDateTimeProvider time)
    {
        this._time = time;
        this._dbConfig = new EmptyDatabaseConfig();
    }
    
    public virtual void Initialize()
    {
        using GameDatabaseContext context = this.GetContext();
        context.Database.Migrate();
    }
    
    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public void Warmup()
    {
        using GameDatabaseContext context = this.GetContext();
        _ = context.GetTotalLevelCount();
    }

    public virtual GameDatabaseContext GetContext()
    {
        return new GameDatabaseContext(this._time, this._dbConfig);
    }
}
