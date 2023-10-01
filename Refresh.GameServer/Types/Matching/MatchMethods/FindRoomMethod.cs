using Bunkum.Core;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using MongoDB.Bson;
using NotEnoughLogs;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Matching.Responses;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Matching.MatchMethods;

public class FindRoomMethod : IMatchMethod
{
    public IEnumerable<string> MethodNames => new[] { "FindBestRoom" };

    public Response Execute(MatchService service, Logger logger, GameDatabaseContext database,
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
                logger.LogWarning(BunkumCategory.Matching, "Received request with invalid amount of slots, rejecting.");
                return BadRequest;
            }
            
            levelId = body.Slots[1];
        } 
        
        //TODO: add user option to filter rooms by language
        
        List<GameRoom> rooms = service.Rooms.Where(r => r.RoomId != usersRoom.RoomId && 
                                                        r.Platform == usersRoom.Platform && 
                                                        (levelId == null || r.LevelId == levelId))
            .OrderByDescending(r => r.RoomMood)
            .ToList();
        
        //When a user is behind a Strict NAT layer, we can only connect them to players with Open NAT types
        if (body.NatType != null && body.NatType[0] == NatType.Strict)
        {
            rooms = rooms.Where(r => r.NatType == NatType.Open).ToList();
        }

        ObjectId? forceMatch = service.GetForceMatch(user.UserId);

        //If the user has a forced match
        if (forceMatch != null)
        {
            //Filter the rooms to only the rooms that contain the player we are wanting to force match to
            rooms = rooms.Where(r => r.PlayerIds.Any(player => player.Id != null && player.Id == forceMatch.Value)).ToList();
        }
        
        if (rooms.Count <= 0)
        {
            return NotFound; // TODO: update this response, shouldn't be 404
        }

        //If the user has a forced match and we found a room
        if (forceMatch != null)
        {
            //Clear the user's force match
            service.ClearForceMatch(user.UserId);
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