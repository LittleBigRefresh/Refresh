using Bunkum.Core;
using Bunkum.Core.Responses;
using NotEnoughLogs;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Matching.MatchMethods;

public class CreateRoomMethod : IMatchMethod
{
    public IEnumerable<string> MethodNames => new[] { "CreateRoom" };

    public Response Execute(MatchService service, Logger logger, GameDatabaseContext database, GameUser user, Token token,
        SerializedRoomData body)
    {
        NatType natType = body.NatType == null ? NatType.Open : body.NatType[0];
        GameRoom room = service.GetOrCreateRoomByPlayer(user, token.TokenPlatform, token.TokenGame, natType, body.BuildVersion, body.PassedNoJoinPoint ?? false);
        if (room.HostId.Id != user.UserId)
        {
            room = service.SplitUserIntoNewRoom(user, token.TokenPlatform, token.TokenGame, natType, body.BuildVersion, body.PassedNoJoinPoint ?? false);
        }

        if (body.RoomState != null) room.RoomState = body.RoomState.Value;
        
        if (body.Slots.Count > 1)
        {
            logger.LogWarning(BunkumCategory.Matching, "Received create room request with multiple slots, rejecting");
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
        
        service.RoomAccessor.UpdateRoom(room);

        return OK;
    }
}