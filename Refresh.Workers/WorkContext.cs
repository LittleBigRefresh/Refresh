using Bunkum.Core.Storage;
using NotEnoughLogs;
using Refresh.Database;

namespace Refresh.Workers;

public class WorkContext : IDataContext
{
    public required GameDatabaseContext Database { get; init; }
    public required Logger Logger { get; init; }
    public required IDataStore DataStore { get; init; }
}