using System.Reflection;
using Bunkum.CustomHttpListener.Request;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Database;
using Bunkum.HttpServer.Responses;
using Bunkum.HttpServer.Services;
using NotEnoughLogs;
using Refresh.GameServer.Database;
using Refresh.GameServer.Endpoints;

namespace Refresh.GameServer.Services;

public class RequestStatisticTrackingService : Service
{
    internal RequestStatisticTrackingService(Logger logger) : base(logger)
    {}

    public override Response? OnRequestHandled(ListenerContext context, MethodInfo method, Lazy<IDatabaseContext> database)
    {
        GameDatabaseContext gameDatabase = (GameDatabaseContext)database.Value;

        if (context.Uri.AbsolutePath.StartsWith(GameEndpointAttribute.BaseRoute))
        {
            gameDatabase.IncrementGameRequests();
        }
        else if(context.Uri.AbsolutePath.StartsWith(LegacyApiEndpointAttribute.BaseRoute))
        {
            gameDatabase.IncrementLegacyApiRequests();
        }
        else
        {
            gameDatabase.IncrementApiRequests();
        }
        
        return null;
    }
}