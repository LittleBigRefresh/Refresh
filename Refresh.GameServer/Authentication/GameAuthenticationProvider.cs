using System.Diagnostics;
using Bunkum.Listener.Request;
using Refresh.Database.Models.Users;
using Bunkum.Core.Authentication;
using Bunkum.Core.Database;
using Refresh.Core.Configuration;
using Refresh.Database.Models.Authentication;
using Refresh.Database;
using Refresh.Interfaces.APIv3;
using Refresh.Interfaces.Game;
using Refresh.Interfaces.Internal;
using NotEnoughLogs;
using Bunkum.Core;

namespace Refresh.GameServer.Authentication;

public class GameAuthenticationProvider : IAuthenticationProvider<Token>
{
    private readonly GameServerConfig? _config;
    private readonly Logger _logger;

    public GameAuthenticationProvider(GameServerConfig? config, Logger logger)
    {
        this._config = config;
        this._logger = logger;
    }

    public Token? AuthenticateToken(ListenerContext request, Lazy<IDatabaseContext> db)
    {
        // Dont attempt to authenticate presence endpoints, as authentication is handled by PresenceAuthenticationMiddleware
        if (request.Uri.AbsolutePath.StartsWith(PresenceEndpointAttribute.BaseRoute))
            return null;
        
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
        
        // Don't allow non-admins to authenticate during maintenance mode.
        // technically, this check isn't here for token but this is okay since
        // we don't actually receive tokens in endpoints (except during logout, aka token revocation)
        if ((this._config?.MaintenanceMode ?? false) && user.Role != GameUserRole.Admin)
            return null;
        
        if (this._config?.PrintAuthenticationData ?? false)
            this._logger.LogInfo(BunkumCategory.Authentication, $"Authenticating request from {request.RemoteEndpoint} to {request.Uri.AbsolutePath} by {user} using token {tokenData}");
        
        // Additional validation of the token gotten from DB. Exceptions will be caught, logged and InternalServerError will be returned automatically.
        if (token.TokenData != tokenData)
        {
#if DEBUG
            if(Debugger.IsAttached) Debugger.Break();
#endif
            throw new InvalidDataException($"{typeof(GameAuthenticationProvider)} - Token from DB ({token.TokenData}) does not match token received from client ({tokenData})!");
        }

        if (token.User.UserId != token.UserId)
        {
#if DEBUG
            if(Debugger.IsAttached) Debugger.Break();
#endif
            throw new InvalidDataException($"{typeof(GameAuthenticationProvider)} - GameUser included with token is not the token owner!");
        }

        return token;
    }
}