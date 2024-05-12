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
        GameRoom? usersRoom = service.RoomAccessor.GetRoomByUser(user, token.TokenPlatform, token.TokenGame);
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
        
        //TODO: Add user option to filter rooms by language
        //TODO: Deprioritize rooms which have PassedNoJoinPoint set 
        //TODO: Filter by BuildVersion
        
        IEnumerable<GameRoom> rooms = service.RoomAccessor
            // Get all the available rooms 
            .GetRoomsByGameAndPlatform(token.TokenGame, token.TokenPlatform)
            .Where(r =>
                // Make sure we don't match the user into their own room
                r.RoomId != usersRoom.RoomId &&
                // If the level id isn't specified, or is 0, then we don't want to try to match against level IDs, else only match the user to people who are playing that level
                (levelId == null || levelId == 0 || r.LevelId == levelId) &&
                // Make sure that we don't try to match the player into a full room, or a room which won't fit the user's current room
                usersRoom.PlayerIds.Count + r.PlayerIds.Count <= 4)
            // Shuffle the rooms around before sorting, this is because the selection is based on a weighted average towards the top of the range,
            // so there would be a bias towards longer lasting rooms without this shuffle
            .OrderBy(r => Random.Shared.Next())
            // Order by descending room mood, so that rooms with higher mood (e.g. allowing more people) get selected more often
            // This is a stable sort, which is why the order needs to be shuffled above
            .ThenByDescending(r => r.RoomMood);
        
        //When a user is behind a Strict NAT layer, we can only connect them to players with Open NAT types
        if (body.NatType != null && body.NatType[0] == NatType.Strict)
        {
            rooms = rooms.Where(r => r.NatType == NatType.Open);
        }

        ObjectId? forceMatch = user.ForceMatch;

        //If the user has a forced match
        if (forceMatch != null)
        {
            //Filter the rooms to only the rooms that contain the player we are wanting to force match to
            rooms = rooms.Where(r => r.PlayerIds.Any(player => player.Id != null && player.Id == forceMatch.Value));
        }
        
        // Now that we've done all our filtering, lets convert it to a list, so we can index it quickly.
        List<GameRoom> roomList = rooms.ToList();
        
        if (roomList.Count <= 0)
        {
            //Return a 404 status code if there's no rooms to match them to
            return new Response(new List<object> { new SerializedStatusCodeMatchResponse(404), }, ContentType.Json);
        }

        // If the user has a forced match and we found a room
        if (forceMatch != null)
        {
            // Clear the user's force match
            database.ClearForceMatch(user);
        }
        
        // Generate a weighted random number, this is weighted relatively strongly towards lower numbers,
        // which makes it more likely to pick rooms with a higher mood, since those are sorted near the start of the array
        // Graph: https://www.desmos.com/calculator/aagcmlbb08
        double weightedRandom = 1 - Math.Cbrt(1 - Random.Shared.NextDouble());
        
        // Even though NextDouble guarantees the result to be < 1.0, and this mathematically always will check out,
        // rounding errors may cause this to become roomList.Count (which would crash), so we use a Math.Min to make sure it doesn't
        GameRoom room = roomList[Math.Min(roomList.Count - 1, (int)Math.Floor(weightedRandom * roomList.Count))];

        SerializedRoomMatchResponse roomMatch = new()
        {
            HostMood = (byte)room.RoomMood,
            RoomState = (byte)room.RoomState,
            Players = [],
            Slots =
            [
                [
                    (byte)room.LevelType,
                    room.LevelId,
                ],
            ],
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

        List<object> response =
        [
            status,
            roomMatch,
        ];

        return new Response(response, ContentType.Json);
    }
}