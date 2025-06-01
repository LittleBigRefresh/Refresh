using Bunkum.Core;
using Bunkum.Core.Responses;
using Refresh.Core.Configuration;
using Refresh.Core.Types.Data;

namespace Refresh.Core.Types.Matching.MatchMethods;

public class UpdateRoomDataMethod : IMatchMethod
{
    public IEnumerable<string> MethodNames => new[] { "UpdateMyPlayerData" };

    public Response Execute(DataContext dataContext, SerializedRoomData body, GameServerConfig gameServerConfig)
    {
        GameRoom room = dataContext.Match.GetOrCreateRoomByPlayer(dataContext.User!, dataContext.Platform, dataContext.Game, body.NatType == null ? NatType.Open : body.NatType[0], body.PassedNoJoinPoint);
        if (room.HostId.Id != dataContext.User!.UserId) return Unauthorized;

        if (body.RoomState != null) room.RoomState = body.RoomState.Value;
        
        // Players cannot be in multiple levels at once
        if (body.Slots.Count > 1)
        {
            dataContext.Logger.LogWarning(BunkumCategory.Matching, "Received update room request with multiple slots, rejecting");
            return BadRequest;
        }
        
        foreach(List<int> slot in body.Slots)
        {
            if (slot.Count != 2)
            {
                dataContext.Logger.LogWarning(BunkumCategory.Matching, "Received request with invalid slot, rejecting.");
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
        
        dataContext.Match.RoomAccessor.UpdateRoom(room);

        return OK;
    }
}