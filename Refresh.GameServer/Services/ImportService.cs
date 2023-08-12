using System.Reflection;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Services;
using NotEnoughLogs;
using Refresh.GameServer.Importing;

namespace Refresh.GameServer.Services;

public class ImportService : Service
{
    internal ImportService(LoggerContainer<BunkumContext> logger) : base(logger)
    {
        this._assetImporter = new AssetImporter(logger);
        this._imageImporter = new ImageImporter(logger);
    }

    private readonly AssetImporter _assetImporter;
    private readonly ImageImporter _imageImporter;

    public override object? AddParameterToEndpoint(ListenerContext context, ParameterInfo paramInfo, Lazy<IDatabaseContext> database)
    {
        if (paramInfo.ParameterType == typeof(AssetImporter))
            return this._assetImporter;

        if (paramInfo.ParameterType == typeof(ImageImporter))
            return this._imageImporter;

        return base.AddParameterToEndpoint(context, paramInfo, database);
    }
}