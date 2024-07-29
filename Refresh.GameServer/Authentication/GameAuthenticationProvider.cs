using System.Diagnostics;
using Bunkum.Listener.Request;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;
using Bunkum.Core.Authentication;
using Bunkum.Core.Database;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Endpoints;
using Refresh.GameServer.Types.Roles;

namespace Refresh.GameServer.Authentication;

public class GameAuthenticationProvider : IAuthenticationProvider<Token>
{
    private readonly GameServerConfig? _config;

    public GameAuthenticationProvider(GameServerConfig? config)
    {
        this._config = config;
    }

    public Token? AuthenticateToken(ListenerContext request, Lazy<IDatabaseContext> db)
    {
        // First try to grab game token data from MM_AUTH
        string? tokenData = request.Cookies["MM_AUTH"];
        TokenType tokenType = TokenType.Game;

        // If this is null, this must be an API request so grab from authorization
        if (tokenData == null)
        {
            tokenType = TokenType.Api;
            tokenData = request.RequestHeaders["Authorization"];
        }
        
        // If still null, then we could not find a token, so bail
        if (tokenData == null) return null;

        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        string validBaseRoute = tokenType switch
        {
            TokenType.Game => GameEndpointAttribute.BaseRoute,
            TokenType.Api => ApiV3EndpointAttribute.BaseRoute,
            _ => throw new ArgumentOutOfRangeException(),
        };

        // If the request URI does not match the base route of the type of token it is, then bail.
        // This is to prevent API tokens from making game requests, and vice versa.
        if (!request.Uri.AbsolutePath.StartsWith(validBaseRoute))
            return null;
        
        GameDatabaseContext database = (GameDatabaseContext)db.Value;
        Debug.Assert(database != null);

        Token? token = database.GetTokenFromTokenData(tokenData, tokenType);
        if (token == null) return null;
        
        // Don't allow game requests from the wrong IP. 
        if (tokenType == TokenType.Game && request.RemoteEndpoint.Address.ToString() != token.IpAddress)
            return null;

        GameUser user = token.User;
        user.RateLimitUserId = user.UserId;
        
        // Don't allow non-admins to authenticate during maintenance mode.
        // technically, this check isn't here for token but this is okay since
        // we don't actually receive tokens in endpoints (except during logout, aka token revocation)
        if ((this._config?.MaintenanceMode ?? false) && user.Role != GameUserRole.Admin)
            return null;

        return token;
    }
}