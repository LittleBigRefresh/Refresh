using Bunkum.Core;
using Bunkum.Core.Database;
using Bunkum.Core.Services;
using Bunkum.Core.Storage;
using Bunkum.Listener.Request;
using NotEnoughLogs;
using Refresh.GameServer.Database;

namespace Refresh.GameServer.Types.Data;

public class DataContextService : Service
{
    private readonly StorageService _storageService;
    
    internal DataContextService(StorageService storage, Logger logger) : base(logger)
    {
        this._storageService = storage;
    }
    
    private static T StealInjection<T>(Service service, string name = "")
    {
        return (T)service.AddParameterToEndpoint(null!, new BunkumParameterInfo(typeof(T), name), null!)!;
    }
    
    public override object? AddParameterToEndpoint(ListenerContext context, BunkumParameterInfo parameter, Lazy<IDatabaseContext> database)
    {
        if (ParameterEqualTo<DataContext>(parameter))
        {
            return new DataContext
            {
                Database = (GameDatabaseContext)database.Value,
                Logger = this.Logger,
                DataStore = StealInjection<IDataStore>(this._storageService),
            };
        }
        
        return null;
    }
}