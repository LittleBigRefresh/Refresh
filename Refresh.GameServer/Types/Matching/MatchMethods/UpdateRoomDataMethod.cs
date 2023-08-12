using Bunkum.HttpServer;
using Bunkum.HttpServer.Responses;
using NotEnoughLogs;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Matching.MatchMethods;

public class UpdateRoomDataMethod : IMatchMethod
{
    public IEnumerable<string> MethodNames => new[] { "UpdateMyPlayerData", "CreateRoom" };

    public Response Execute(MatchService service, LoggerContainer<BunkumContext> logger,
        GameDatabaseContext database, GameUser user, Token token, SerializedRoomData body)
    {
        GameRoom room = service.GetOrCreateRoomByPlayer(user, token.TokenPlatform, token.TokenGame);
        if (room.HostId.Id != user.UserId) return Unauthorized;

        if (body.RoomState != null) room.RoomState = body.RoomState.Value;

        // LBP likes to send both Slot and Slots interchangeably, handle that case here
        List<int>? slot = body.Slot ?? body.Slots;
        if (slot != null)
        {
            if (slot.Count != 2)
            {
                logger.LogWarning(BunkumContext.Matching, "Received request with invalid amount of slots, rejecting.");
                return BadRequest;
            }

            room.LevelType = (RoomSlotType)slot[0];
            room.LevelId = slot[1];
        }

        byte? mood = body.HostMood ?? body.Mood;
        if (mood != null)
        {
            room.RoomMood = (RoomMood)mood;
        }

        return OK;
    }
}