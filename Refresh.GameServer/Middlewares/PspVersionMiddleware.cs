using Bunkum.Listener.Request;
using Bunkum.Core.Database;
using Bunkum.Core.Endpoints.Middlewares;

namespace Refresh.GameServer.Middlewares;

public class PspVersionMiddleware : IMiddleware
{
    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        string? exeVersion = context.RequestHeaders.Get("X-exe-v");
        string? dataVersion = context.RequestHeaders.Get("X-data-v");

        if (exeVersion != null)
        {
            context.ResponseHeaders["X-exe-v"] = exeVersion;
        }

        if (dataVersion != null)
        {
            context.ResponseHeaders["X-data-v"] = dataVersion;
        }
        
        next();
    }
}