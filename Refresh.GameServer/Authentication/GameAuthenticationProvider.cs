using System.Diagnostics;
using Bunkum.CustomHttpListener.Request;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;
using Bunkum.HttpServer.Authentication;
using Bunkum.HttpServer.Database;

namespace Refresh.GameServer.Authentication;

public class GameAuthenticationProvider : IAuthenticationProvider<GameUser, Token>
{
    public GameUser? AuthenticateUser(ListenerContext request, Lazy<IDatabaseContext> database) 
        => this.AuthenticateToken(request, database)?.User;

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

        return database.GetTokenFromTokenData(tokenData, tokenType);
    }
}