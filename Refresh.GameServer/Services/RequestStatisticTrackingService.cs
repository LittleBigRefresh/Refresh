using System.Reflection;
using Bunkum.Listener.Request;
using Bunkum.Core.Database;
using Bunkum.Core.Responses;
using Bunkum.Core.Services;
using NotEnoughLogs;
using Refresh.GameServer.Endpoints;

namespace Refresh.GameServer.Services;

public class RequestStatisticTrackingService : Service
{
    private static int _gameRequestsToSubmit;
    private static int _apiRequestsToSubmit;
    
    public static (int game, int api) SubmitAndClearRequests()
    {
        return (game: Interlocked.Exchange(ref _gameRequestsToSubmit, 0),
                api: Interlocked.Exchange(ref _apiRequestsToSubmit, 0));
    }
    
    internal RequestStatisticTrackingService(Logger logger) : base(logger)
    {}

    public override Response? OnRequestHandled(ListenerContext context, MethodInfo method, Lazy<IDatabaseContext> database)
    {
        if (context.Uri.AbsolutePath.StartsWith(GameEndpointAttribute.BaseRoute))
            Interlocked.Increment(ref _gameRequestsToSubmit);
        else
            Interlocked.Increment(ref _apiRequestsToSubmit);
        
        return null;
    }
}