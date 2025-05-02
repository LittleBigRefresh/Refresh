using Bunkum.Core.Storage;
using NotEnoughLogs;
using Refresh.Database;

namespace Refresh.Database;

public interface IDataContext
{
    GameDatabaseContext Database { get; }
    Logger Logger { get; }
    IDataStore DataStore { get; }
}