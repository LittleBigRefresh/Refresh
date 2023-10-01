using Bunkum.Listener.Request;
using Bunkum.Core.Database;
using Bunkum.Core.Endpoints.Middlewares;

namespace Refresh.GameServer.Middlewares;

public class LegacyAdapterMiddleware : IMiddleware
{
    private const string OldUrl = "/LITTLEBIGPLANETPS3_XML";
    private const string NewUrl = "/lbp";
    
    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        if (!context.Uri.AbsolutePath.StartsWith(OldUrl))
        {
            next();
            return;
        }

        context.Uri = new Uri(context.Uri, context.Uri.AbsolutePath.Replace(OldUrl, NewUrl));

        next();
    }
}