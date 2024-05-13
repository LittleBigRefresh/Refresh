using Bunkum.Core.Storage;
using NotEnoughLogs;
using Refresh.GameServer.Database;

namespace Refresh.GameServer.Types.Data;

public class DataContext
{
    public required GameDatabaseContext Database;
    public required Logger Logger;
    public required IDataStore DataStore;
}