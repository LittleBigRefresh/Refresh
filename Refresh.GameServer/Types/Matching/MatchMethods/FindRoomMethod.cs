using System.Net;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Responses;
using NotEnoughLogs;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Matching.MatchMethods;

public class FindRoomMethod : IMatchMethod
{
    public IEnumerable<string> MethodNames => new[] { "FindBestRoom" };

    public Response Execute(MatchService service, LoggerContainer<BunkumContext> logger, GameDatabaseContext database, GameUser user,
        SerializedRoomData body)
    {
        GameRoom? usersRoom = service.GetRoomByPlayer(database, user);
        if (usersRoom == null) return HttpStatusCode.BadRequest; // user should already have a room.

        List<GameRoom> rooms = service.Rooms.Where(r => r.RoomId != usersRoom.RoomId)
            .OrderByDescending(r => r.RoomMood)
            .ToList();

        if (rooms.Count <= 0)
        {
            return HttpStatusCode.NotFound; // TODO: update this response, shouldn't be 404
        }

        return HttpStatusCode.OK;
    }
}