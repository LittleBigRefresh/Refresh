using Bunkum.Core.Responses;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Types.Data;
using Refresh.Database.Models.Users;

namespace Refresh.GameServer.Types.Matching.MatchMethods;

public class UpdatePlayersInRoomMethod : IMatchMethod
{
    public IEnumerable<string> MethodNames => new[] { "UpdatePlayersInRoom" };

    public Response Execute(DataContext dataContext, SerializedRoomData body, GameServerConfig gameServerConfig)
    {
        if (body.Players == null) return BadRequest;
        GameRoom room = dataContext.Match.GetOrCreateRoomByPlayer(dataContext.User!, dataContext.Platform, dataContext.Game, body.NatType == null ? NatType.Open : body.NatType[0], body.PassedNoJoinPoint);
        
        foreach (string playerUsername in body.Players)
        {
            GameUser? player = dataContext.Database.GetUserByUsername(playerUsername);
            
            if (player != null)
                dataContext.Match.AddPlayerToRoom(player, room, dataContext.Platform, dataContext.Game);
            else
                dataContext.Match.AddPlayerToRoom(playerUsername, room);
        }

        return OK;
    }
}