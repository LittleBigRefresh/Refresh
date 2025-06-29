using System.Diagnostics.CodeAnalysis;
using NotEnoughLogs;
using Refresh.Common.Time;
using Refresh.Database.Configuration;

namespace Refresh.Database;

public class GameDatabaseProvider : 
    IDatabaseProvider<GameDatabaseContext>
{
    private readonly IDateTimeProvider _time;
    private readonly IDatabaseConfig _dbConfig;
    protected readonly Logger Logger;

    public GameDatabaseProvider(Logger logger, IDatabaseConfig dbConfig)
    {
        this.Logger = logger;
        this._time = new SystemDateTimeProvider();
        this._dbConfig = dbConfig;
    }

    protected GameDatabaseProvider(Logger logger, IDateTimeProvider time)
    {
        this.Logger = logger;
        this._time = time;
        this._dbConfig = new EmptyDatabaseConfig();
    }
    
    [SuppressMessage("ReSharper.DPA", "DPA0005: Database issues")]
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
        return new GameDatabaseContext(this.Logger, this._time, this._dbConfig);
    }
}
