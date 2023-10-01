using Bunkum.Listener.Request;
using Bunkum.Core.Database;
using Bunkum.Core.Endpoints.Middlewares;

namespace Refresh.GameServer.Middlewares;

public class ApiV2GoneMiddleware : IMiddleware
{
    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        if (context.Uri.AbsolutePath.StartsWith("/api/v2"))
        {
            context.ResponseCode = Gone;
            context.ResponseHeaders.Add("Content-Type", "application/json");
            context.Write("{\"error\": \"APIv2 is deprecated and removed. Please migrate to APIv3 or use /api/v1 for Lighthouse applications." +
                          " See /api/v3/documentation or /docs on refresh-web for further instructions.\",\"success\": false}");
            return;
        }

        next();
    }
}