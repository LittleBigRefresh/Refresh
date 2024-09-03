using Bunkum.Core.Authentication;
using Bunkum.Core.Database;
using Bunkum.Listener.Request;
using Refresh.PresenceServer.Server.Config;

namespace Refresh.PresenceServer.ApiServer.Authentication;

public class SecretAuthenticationProvider : IAuthenticationProvider<Token>
{
    private PresenceServerConfig _config;

    public SecretAuthenticationProvider(PresenceServerConfig config)
    {
        this._config = config;
    }

    public Token? AuthenticateToken(ListenerContext request, Lazy<IDatabaseContext> database)
    {
        return request.RequestHeaders["Authorization"] == this._config.SharedSecret ? new Token() : null;
    }
}