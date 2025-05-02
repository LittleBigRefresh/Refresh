using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Protocols.Http;
using Refresh.GameServer.Authentication;
using Refresh.Database;
using Refresh.GameServer.Types.UserData;
using GameDatabaseContext = Refresh.Database.GameDatabaseContext;

namespace Refresh.GameServer.Endpoints.Internal.Presence;

public class PresenceEndpoints : EndpointGroup
{
    // Test endpoint to allow the presence server to make sure presence support is enabled and configured correctly
    [PresenceEndpoint("test", HttpMethods.Post), Authentication(false)]
    public Response TestSecret(RequestContext context)
    {
        return OK;
    }

    [PresenceEndpoint("informConnection", HttpMethods.Post), Authentication(false)]
    public Response InformConnection(RequestContext context, GameDatabaseContext database, string body)
    {
        GameUser? user = database.GetUserFromTokenData(body, TokenType.Game);

        if (user == null)
            return NotFound;
        
        database.SetUserPresenceAuthToken(user, body);

        context.Logger.LogInfo(RefreshContext.Presence, $"User {user} connected to the presence server");
        
        return OK;
    }

    [PresenceEndpoint("informDisconnection", HttpMethods.Post), Authentication(false)]
    public Response InformDisconnection(RequestContext context, GameDatabaseContext database, string body)
    {
        GameUser? user = database.GetUserFromTokenData(body, TokenType.Game);

        if (user == null)
            return NotFound;
        
        database.SetUserPresenceAuthToken(user, null);

        context.Logger.LogInfo(RefreshContext.Presence, $"User {user} disconnected from the presence server");

        return OK;
    }
}