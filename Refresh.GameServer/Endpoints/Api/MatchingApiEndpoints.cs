using Bunkum.HttpServer;
using Bunkum.HttpServer.Endpoints;
using MongoDB.Bson;
using Refresh.GameServer.Database;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Matching;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Endpoints.Api;

public class MatchingApiEndpoints : EndpointGroup
{
    [ApiEndpoint("room/username/{username}")]
    [Authentication(false)]
    [NullStatusCode(NotFound)]
    public GameRoom? GetRoomByUsername(RequestContext context, MatchService service, GameDatabaseContext database,
        string username)
    {
        GameUser? user = database.GetUserByUsername(username);
        if (user == null) return null;

        return service.GetRoomByPlayer(user);
    }
    
    [ApiEndpoint("room/uuid/{uuid}")]
    [Authentication(false)]
    [NullStatusCode(NotFound)]
    public GameRoom? GetRoomByUserUuid(RequestContext context, MatchService service, GameDatabaseContext database,
        string uuid)
    {
        GameUser? user = database.GetUserByUuid(uuid);
        if (user == null) return null;

        return service.GetRoomByPlayer(user);
    }
    
    [ApiEndpoint("room/{uuid}")]
    [Authentication(false)]
    [NullStatusCode(NotFound)]
    public GameRoom? GetRoomByUuid(RequestContext context, MatchService service, string uuid)
    {
        bool parsed = ObjectId.TryParse(uuid, out ObjectId objectId);
        if (!parsed) return null;
        
        return service.Rooms.FirstOrDefault(r => r.RoomId == objectId);
    }
    
    [ApiEndpoint("rooms")]
    [Authentication(false)]
    public IEnumerable<GameRoom> GetRooms(RequestContext context, MatchService service)
    {
        (int skip, int count) = context.GetPageData(true);
        return service.Rooms.Skip(skip).Take(count);
    }
}