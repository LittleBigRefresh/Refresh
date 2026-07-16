using Bunkum.Core.Storage;
using NotEnoughLogs;
using Refresh.Common.Time;
using Refresh.Database;

namespace Refresh.Workers;

public class WorkContext : IDataContext
{
    public required GameDatabaseContext Database { get; init; }
    public required Logger Logger { get; init; }
    public required IDataStore DataStore { get; init; }
    // TODO also use this in jobs outside of NewUserJob which also rely on current time
    public required IDateTimeProvider TimeProvider { get; init; }
}