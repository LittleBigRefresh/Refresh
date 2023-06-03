using System.Reflection;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Services;
using NotEnoughLogs;

namespace Refresh.GameServer.Services;

public class AssetImportService : Service
{
    internal AssetImportService(LoggerContainer<BunkumContext> logger) : base(logger)
    {
        this._importer = new AssetImporter(logger);
    }

    private readonly AssetImporter _importer;

    public override object? AddParameterToEndpoint(ListenerContext context, ParameterInfo paramInfo, Lazy<IDatabaseContext> database)
    {
        if (paramInfo.ParameterType == typeof(AssetImporter))
            return this._importer;

        return base.AddParameterToEndpoint(context, paramInfo, database);
    }
}