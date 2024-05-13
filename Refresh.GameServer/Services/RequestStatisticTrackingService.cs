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
    public static readonly object TrackerLock = new();
    
    public static int GameRequestsToSubmit { get; private set; }
    public static int ApiRequestsToSubmit { get; private set; }
    
    public static void ClearRequests()
    {
        GameRequestsToSubmit = 0;
        ApiRequestsToSubmit = 0;
    }
    
    internal RequestStatisticTrackingService(Logger logger) : base(logger)
    {}

    public override Response? OnRequestHandled(ListenerContext context, MethodInfo method, Lazy<IDatabaseContext> database)
    {
        lock (TrackerLock)
        {
            if (context.Uri.AbsolutePath.StartsWith(GameEndpointAttribute.BaseRoute))
            {
                GameRequestsToSubmit++;
            }
            else
            {
                ApiRequestsToSubmit++;
            }
        }
        
        return null;
    }
}