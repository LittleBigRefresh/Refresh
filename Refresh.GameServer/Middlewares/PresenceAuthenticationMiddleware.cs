using Bunkum.Core.Database;
using Bunkum.Core.Endpoints.Middlewares;
using Bunkum.Listener.Request;
using Refresh.Core.Configuration;
using Refresh.GameServer.Endpoints;

namespace Refresh.GameServer.Middlewares;

public class PresenceAuthenticationMiddleware : IMiddleware
{
    private IntegrationConfig _config;

    public PresenceAuthenticationMiddleware(IntegrationConfig config)
    {
        this._config = config;
    }

    public void HandleRequest(ListenerContext context, Lazy<IDatabaseContext> database, Action next)
    {
        if (!context.Uri.AbsolutePath.StartsWith(PresenceEndpointAttribute.BaseRoute))
        {
            next();
            return;
        }

        // Block presence requests if not enabled
        if (!this._config.PresenceEnabled)
        {
            context.ResponseCode = NotImplemented;
            return;
        }

        // Block presence requests with a bad auth token
        if (context.RequestHeaders["Authorization"] != this._config.PresenceSharedSecret)
        {
            context.ResponseCode = Unauthorized;
            return;
        }

        next();
    }
}