using Bunkum.Core.Database;
using Bunkum.Core.Endpoints.Middlewares;
using Bunkum.Listener.Request;
using Refresh.PresenceServer.Server.Config;

namespace Refresh.PresenceServer.ApiServer.Middlewares;

public class SharedSecretAuthMiddleware : IMiddleware
{
    private readonly PresenceServerConfig _config;

    public SharedSecretAuthMiddleware(PresenceServerConfig config)
    {
        this._config = config;
    }

    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        if (context.RequestHeaders["Authorization"] != this._config.SharedSecret)
        {
            context.ResponseCode = Unauthorized;
            return;
        }
        
        next();
    }
}