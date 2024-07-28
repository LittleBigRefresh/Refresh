using Bunkum.Core;
using Bunkum.Core.Responses;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Types.Data;

namespace Refresh.GameServer.Types.Matching.MatchMethods;

public class CreateRoomMethod : IMatchMethod
{
    public IEnumerable<string> MethodNames => new[] { "CreateRoom" };

    public Response Execute(DataContext dataContext, SerializedRoomData body, GameServerConfig gameServerConfig)
    {
        NatType natType = body.NatType == null ? NatType.Open : body.NatType[0];
        GameRoom room = dataContext.Match.GetOrCreateRoomByPlayer(dataContext.User!, dataContext.Platform, dataContext.Game, natType, body.PassedNoJoinPoint);
        if (room.HostId.Id != dataContext.User!.UserId)
        {
            room = dataContext.Match.SplitUserIntoNewRoom(dataContext.User, dataContext.Platform, dataContext.Game, natType, body.PassedNoJoinPoint);
        }

        if (body.RoomState != null) room.RoomState = body.RoomState.Value;
        
        if (body.Slots.Count > 1)
        {
            dataContext.Logger.LogWarning(BunkumCategory.Matching, "Received create room request with multiple slots, rejecting");
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
        
        dataContext.Match.RoomAccessor.UpdateRoom(room);

        return OK;
    }
}