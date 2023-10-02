using System.Diagnostics;
using Bunkum.Listener.Request;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;
using Bunkum.Core.Authentication;
using Bunkum.Core.Database;
using Refresh.GameServer.Configuration;
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
        // first try to grab token data from MM_AUTH
        string? tokenData = request.Cookies["MM_AUTH"];
        TokenType tokenType = TokenType.Game;

        // if this is null, this must be an API request so grab from authorization
        if (tokenData == null)
        {
            tokenType = TokenType.Api;
            tokenData = request.RequestHeaders["Authorization"];
        }

        // if still null we dont have a token so bail 
        if (tokenData == null) return null;
        
        GameDatabaseContext database = (GameDatabaseContext)db.Value;
        Debug.Assert(database != null);

        Token? token = database.GetTokenFromTokenData(tokenData, tokenType);
        if (token == null) return null;

        GameUser user = token.User;
        user.RateLimitUserId = user.UserId;
        
        // don't allow non-admins to authenticate during maintenance mode.
        // technically, this check isn't here for token but this is okay since
        // we don't actually receive tokens in endpoints (except during logout, aka token revocation)
        if ((this._config?.MaintenanceMode ?? false) && user.Role != GameUserRole.Admin)
            return null;

        return token;
    }
}