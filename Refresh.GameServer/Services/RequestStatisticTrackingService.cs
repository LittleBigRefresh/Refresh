using System.Reflection;
using Bunkum.Listener.Request;
using Bunkum.Core.Database;
using Bunkum.Core.Responses;
using Bunkum.Core.Services;
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
        else
        {
            gameDatabase.IncrementApiRequests();
        }
        
        return null;
    }
}