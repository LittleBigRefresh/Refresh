using Bunkum.Core;
using Bunkum.Core.Responses;
using NotEnoughLogs;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Matching.MatchMethods;

public class UpdateRoomDataMethod : IMatchMethod
{
    public IEnumerable<string> MethodNames => new[] { "UpdateMyPlayerData" };

    public Response Execute(MatchService service, Logger logger,
        GameDatabaseContext database, GameUser user, Token token, SerializedRoomData body)
    {
        GameRoom room = service.GetOrCreateRoomByPlayer(user, token.TokenPlatform, token.TokenGame, body.NatType == null ? NatType.Open : body.NatType[0]);
        if (room.HostId.Id != user.UserId) return Unauthorized;

        if (body.RoomState != null) room.RoomState = body.RoomState.Value;

        // LBP likes to send both Slot and Slots interchangeably, handle that case here
        if (body.Slots != null)
        {
            if (body.Slots.Count != 2)
            {
                logger.LogWarning(BunkumCategory.Matching, "Received request with invalid amount of slots, rejecting.");
                return BadRequest;
            }

            room.LevelType = (RoomSlotType)body.Slots[0];
            room.LevelId = body.Slots[1];
        }

        byte? mood = body.HostMood ?? body.Mood;
        if (mood != null)
        {
            room.RoomMood = (RoomMood)mood;
        }
        
        service.RoomAccessor.UpdateRoom(room);

        return OK;
    }
}