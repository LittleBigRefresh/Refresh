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
        GameRoom room = service.GetOrCreateRoomByPlayer(user, token.TokenPlatform, token.TokenGame, body.NatType == null ? NatType.Open : body.NatType[0], body.BuildVersion, body.PassedNoJoinPoint);
        if (room.HostId.Id != user.UserId) return Unauthorized;

        if (body.RoomState != null) room.RoomState = body.RoomState.Value;
        
        // Players cannot be in multiple levels at once
        if (body.Slots.Count > 1)
        {
            logger.LogWarning(BunkumCategory.Matching, "Received update room request with multiple slots, rejecting");
            return BadRequest;
        }
        
        foreach(List<int> slot in body.Slots)
        {
            if (slot.Count != 2)
            {
                logger.LogWarning(BunkumCategory.Matching, "Received request with invalid slot, rejecting.");
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
        
        if (body.PassedNoJoinPoint != null)
        {
            room.PassedNoJoinPoint = body.PassedNoJoinPoint.Value;
        }
        
        service.RoomAccessor.UpdateRoom(room);

        return OK;
    }
}