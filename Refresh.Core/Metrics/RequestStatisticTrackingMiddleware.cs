using System.Diagnostics;
using Bunkum.Core.Database;
using Bunkum.Core.Endpoints.Middlewares;
using Bunkum.Listener.Request;

namespace Refresh.Core.Metrics;

public class RequestStatisticTrackingMiddleware : IMiddleware
{
    private static int _gameRequestsToSubmit;
    private static int _apiRequestsToSubmit;
    
    public static (int game, int api) SubmitAndClearRequests()
    {
        return (game: Interlocked.Exchange(ref _gameRequestsToSubmit, 0),
            api: Interlocked.Exchange(ref _apiRequestsToSubmit, 0));
    }
    
    private readonly ThreadLocal<Stopwatch> _sw = new(() => new Stopwatch()); 
    
    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        Debug.Assert(this._sw.Value != null);
        this._sw.Value.Start();
        next();
        this._sw.Value.Stop();
        
        if (context.Uri.AbsolutePath.StartsWith("/lbp/" /* GameEndpointAttribute.BaseRoute */))
        {
            Interlocked.Increment(ref _gameRequestsToSubmit);
            GameServerMetrics.RecordGameRequest(this._sw.Value);
        }
        else
        {
            Interlocked.Increment(ref _apiRequestsToSubmit);
            GameServerMetrics.RecordApiRequest(this._sw.Value);
        }
    }
}