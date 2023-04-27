using System.Net;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Legacy;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.LegacyApi;

public class LegacyApiEndpoints : EndpointGroup
{
    [LegacyApiEndpoint("username/{username}")]
    [Authentication(false)]
    public LegacyGameUser? GetLegacyUserByUsername(RequestContext context, GameDatabaseContext database, string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return null;
        
        return LegacyGameUser.FromGameUser(user);
    }
    
    [LegacyApiEndpoint("user/{idStr}")]
    [Authentication(false)]
    public LegacyGameUser? GetLegacyUserByLegacyId(RequestContext context, GameDatabaseContext database, string idStr)
    {
        int.TryParse(idStr, out int id);
        if (id == default) return null;
        
        GameUser? user = database.GetUserByLegacyId(id);
        if (user == null) return null;
        
        return LegacyGameUser.FromGameUser(user);
    }

    [LegacyApiEndpoint("user/{idStr}/status")]
    [Authentication(false)]
    public LegacyStatus? GetLegacyUserStatus(RequestContext context, GameDatabaseContext database, string idStr)
    {
        int.TryParse(idStr, out int id);
        if (id == default) return null;

        return new LegacyStatus();
    }
}