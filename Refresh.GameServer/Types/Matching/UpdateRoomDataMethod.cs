using System.Net;
using System.Text.RegularExpressions;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Responses;
using NotEnoughLogs;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Matching;

public class UpdateRoomDataMethod : IMatchMethod
{
    public IEnumerable<string> MethodNames => new[] { "UpdateMyPlayerData", "CreateRoom" };

    public Response Execute(MatchService service, LoggerContainer<BunkumContext> logger,
        GameDatabaseContext database, GameUser user, SerializedRoomData body)
    {
        GameRoom room = service.GetOrCreateRoomByPlayer(database, user);

        if (body.RoomState != null) room.RoomState = body.RoomState.Value;
        if (body.Slots != null)
        {
            if (body.Slots.Count != 2)
            {
                logger.LogWarning(BunkumContext.Matching, "Received request with invalid amount of slots, rejecting.");
                return HttpStatusCode.BadRequest;
            }

            room.LevelType = (RoomSlotType)body.Slots[0];
            room.LevelId = body.Slots[1];
        }

        return HttpStatusCode.OK;
    }
}