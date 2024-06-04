using Bunkum.Core.Responses;
using NotEnoughLogs;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Matching.MatchMethods;

public class UpdatePlayersInRoomMethod : IMatchMethod
{
    public IEnumerable<string> MethodNames => new[] { "UpdatePlayersInRoom" };

    public Response Execute(MatchService service, Logger logger, GameDatabaseContext database,
        GameUser user,
        Token token,
        SerializedRoomData body)
    {
        if (body.Players == null) return BadRequest;
        GameRoom room = service.GetOrCreateRoomByPlayer(user, token.TokenPlatform, token.TokenGame, body.NatType == null ? NatType.Open : body.NatType[0], body.BuildVersion, body.PassedNoJoinPoint);
        
        foreach (string playerUsername in body.Players)
        {
            GameUser? player = database.GetUserByUsername(playerUsername);
            
            if (player != null)
                service.AddPlayerToRoom(player, room, token.TokenPlatform, token.TokenGame);
            else
                service.AddPlayerToRoom(playerUsername, room);
        }

        return OK;
    }
}