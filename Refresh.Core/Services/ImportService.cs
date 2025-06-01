using Bunkum.Core;
using Bunkum.Listener.Request;
using Bunkum.Core.Database;
using Bunkum.Core.Services;
using NotEnoughLogs;
using Refresh.GameServer.Importing;

namespace Refresh.GameServer.Services;

public class ImportService : Service
{
    internal ImportService(Logger logger, TimeProviderService timeProvider) : base(logger)
    {
        this.AssetImporter = new AssetImporter(logger, timeProvider.TimeProvider);
        this.ImageImporter = new ImageImporter(logger);
    }

    internal readonly AssetImporter AssetImporter;
    internal readonly ImageImporter ImageImporter;

    public override object? AddParameterToEndpoint(ListenerContext context, BunkumParameterInfo paramInfo, Lazy<IDatabaseContext> database)
    {
        if (paramInfo.ParameterType == typeof(AssetImporter))
            return this.AssetImporter;

        if (paramInfo.ParameterType == typeof(ImageImporter))
            return this.ImageImporter;

        return base.AddParameterToEndpoint(context, paramInfo, database);
    }
}