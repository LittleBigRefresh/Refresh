using Bunkum.CustomHttpListener.Parsing;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Responses;
using NotEnoughLogs;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Matching.Responses;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Matching.MatchMethods;

public class FindRoomMethod : IMatchMethod
{
    public IEnumerable<string> MethodNames => new[] { "FindBestRoom" };

    public Response Execute(MatchService service, LoggerContainer<BunkumContext> logger, GameDatabaseContext database,
        GameUser user,
        Token token,
        SerializedRoomData body)
    {
        GameRoom? usersRoom = service.GetRoomByPlayer(user, token.TokenPlatform, token.TokenGame);
        if (usersRoom == null) return BadRequest; // user should already have a room.

        int? levelId = null;
        if (body.Slots != null)
        {
            if (body.Slots.Count != 2)
            {
                return BadRequest;
            }
            
            levelId = body.Slots[1];
        } 
        
        List<GameRoom> rooms = service.Rooms.Where(r => r.RoomId != usersRoom.RoomId && 
                                                        r.Platform == usersRoom.Platform && 
                                                        (levelId == null || r.LevelId == levelId))
            .OrderByDescending(r => r.RoomMood)
            .ToList();

        if (rooms.Count <= 0)
        {
            return NotFound; // TODO: update this response, shouldn't be 404
        }
        
        GameRoom room = rooms[Random.Shared.Next(0, rooms.Count)];

        SerializedRoomMatchResponse roomMatch = new()
        {
            HostMood = (byte)room.RoomMood,
            RoomState = (byte)room.RoomState,
            Players = new List<SerializedRoomPlayer>(),
            Slots = new List<List<int>>(1)
            {
                new(1)
                {
                    (byte)room.LevelType,
                    room.LevelId,
                },
            },
        };
        
        foreach (GameUser? roomUser in room.GetPlayers(database))
        {
            if(roomUser == null) continue;
            roomMatch.Players.Add(new SerializedRoomPlayer(roomUser.Username, 0));
        }
        
        foreach (GameUser? roomUser in usersRoom.GetPlayers(database))
        {
            if(roomUser == null) continue;
            roomMatch.Players.Add(new SerializedRoomPlayer(roomUser.Username, 1));
        }

        SerializedStatusCodeMatchResponse status = new(200);

        List<object> response = new(2)
        {
            status,
            roomMatch,
        };

        return new Response(response, ContentType.Json);
    }
}