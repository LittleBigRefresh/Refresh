using MongoDB.Bson;
using Refresh.Database.Models.Authentication;

namespace Refresh.GameServer.Types.Matching.RoomAccessors;

public class NullRoomAccessor : IRoomAccessor
{
    public IEnumerable<GameRoom> GetAllRooms()
    {
        return [];
    }

    public GameRoom? GetRoomByUuid(ObjectId uuid)
    {
        return null;
    }

    public void AddRoom(GameRoom room)
    {}

    public void RemoveRoom(ObjectId uuid)
    {}

    public void UpdateRoom(GameRoom room)
    {}

    public RoomStatistics GetStatistics()
    {
        return new RoomStatistics
        {
            PerGame = [],
            PerPlatform = [],
            PlayerCount = 0,
            PlayersInPodCount = 0,
            RoomCount = 0,
        };
    }

    public ushort GetPlayersInGame(TokenGame game)
    {
        return 0;
    }

    public ushort GetPlayersOnPlatform(TokenPlatform platform)
    {
        return 0;
    }

    public IEnumerable<GameRoom> GetRoomsInLevel(RoomSlotType type, int levelId)
    {
        return [];
    }

    public IEnumerable<GameRoom> GetRoomsByGameAndPlatform(TokenGame game, TokenPlatform platform)
    {
        return [];
    }

    public GameRoom? GetRoomByUserUuid(ObjectId uuid, TokenPlatform? platform = null, TokenGame? game = null)
    {
        return null;
    }

    public GameRoom? GetRoomByUsername(string username, TokenPlatform? platform = null, TokenGame? game = null)
    {
        return null;
    }
}