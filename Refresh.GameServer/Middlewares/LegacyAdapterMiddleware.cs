using Bunkum.Listener.Request;
using Bunkum.Core.Database;
using Bunkum.Core.Endpoints.Middlewares;
using Refresh.Interfaces.Game;

namespace Refresh.GameServer.Middlewares;

public class LegacyAdapterMiddleware : IMiddleware
{
    public const string OldBaseRoute = "/LITTLEBIGPLANETPS3_XML/";
    private const string NewBaseRoute = GameEndpointAttribute.BaseRoute;
    
    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        if (!context.Uri.AbsolutePath.StartsWith(OldBaseRoute))
        {
            next();
            return;
        }

        context.Uri = new Uri(context.Uri, string.Concat(NewBaseRoute, context.Uri.AbsolutePath.AsSpan()[OldBaseRoute.Length..]));

        next();
    }
}