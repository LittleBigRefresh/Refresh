using Bunkum.Core;
using MongoDB.Bson;
using NotEnoughLogs;
using Refresh.Database.Models.Authentication;

namespace Refresh.GameServer.Types.Matching.RoomAccessors;

public class InMemoryRoomAccessor(Logger logger) : IRoomAccessor
{
    private readonly List<GameRoom> _rooms = [];

    /// <summary>
    /// Removes all expired rooms from the list
    /// </summary>
    private void RemoveExpiredRooms()
    {
        int removed = this._rooms.RemoveAll(r => r.IsExpired);
        if (removed == 0) return;
        logger.LogDebug(BunkumCategory.Matching, $"Removed {removed} expired rooms");
    }
    
    /// <inheritdoc/>
    public IEnumerable<GameRoom> GetAllRooms()
    {
        lock (this._rooms)
        {
            this.RemoveExpiredRooms();
            //Return a copy of the rooms, since List is not thread safe in C#
            return new List<GameRoom>(this._rooms);
        }
    }
    
    /// <inheritdoc/>
    public GameRoom? GetRoomByUuid(ObjectId uuid)
    {
        lock (this._rooms)
        {
            this.RemoveExpiredRooms();
            return this._rooms.FirstOrDefault(r => r.RoomId == uuid);
        }
    }
    
    /// <inheritdoc/>
    public void AddRoom(GameRoom room)
    {
        lock (this._rooms)
        {
            this.RemoveExpiredRooms();
            
            //Remove any existing rooms that match the UUID, this is just a precaution against duplicate rooms
            this._rooms.RemoveAll(r => r.RoomId == room.RoomId);
            //Update last contact time
            room.LastContact = DateTimeOffset.Now;
            this._rooms.Add(room);
        }
    }
    
    /// <inheritdoc/>
    public void RemoveRoom(ObjectId uuid)
    {
        lock (this._rooms)
        {
            this.RemoveExpiredRooms();
            this._rooms.RemoveAll(r => r.RoomId == uuid);
        }
    }
    
    /// <inheritdoc/>
    //NOTE: we forward this to `AddRoom`, since that does the same logic we would
    public void UpdateRoom(GameRoom room) => this.AddRoom(room);

    /// <inheritdoc/>
    public RoomStatistics GetStatistics()
    {
        lock (this._rooms)
        {
            //Clear all expired rooms
            this.RemoveExpiredRooms();

            Dictionary<TokenGame, ushort> perGame = new();
            Dictionary<TokenPlatform, ushort> perPlatform = new();
            foreach (GameRoom room in this._rooms)
            {
                perGame.TryAdd(room.Game, 0);
                perPlatform.TryAdd(room.Platform, 0);
                
                perGame[room.Game] += (ushort)room.PlayerIds.Count;
                perPlatform[room.Platform] += (ushort)room.PlayerIds.Count;
            }
            
            return new RoomStatistics
            {
                PlayerCount = (ushort)this._rooms.Sum(r => r.PlayerIds.Count),
                PlayersInPodCount = (ushort)this._rooms.Where(r => r.LevelType == RoomSlotType.Pod).Sum(r => r.PlayerIds.Count),
                RoomCount = (ushort)this._rooms.Count,
                PerGame = perGame,
                PerPlatform = perPlatform,
            };
        }
    }
    
    /// <inheritdoc/>
    public ushort GetPlayersInGame(TokenGame game)
    {
        lock (this._rooms)
        {
            //Clear all expired rooms
            this.RemoveExpiredRooms();

            return (ushort)this._rooms.Sum(r => r.PlayerIds.Count);
        }
    }
    
    /// <inheritdoc/>
    public ushort GetPlayersOnPlatform(TokenPlatform platform)
    {
        lock (this._rooms)
        {
            //Clear all expired rooms
            this.RemoveExpiredRooms();

            return (ushort)this._rooms.Where(r => r.Platform == platform).Sum(r => r.PlayerIds.Count);
        }
    }
    
    /// <inheritdoc/>
    public IEnumerable<GameRoom> GetRoomsInLevel(RoomSlotType type, int levelId)
    {
        lock (this._rooms)
        {
            //Clear all expired rooms
            this.RemoveExpiredRooms();

            //Return a copy of the room list
            return this._rooms.Where(r => r.LevelId == levelId && r.LevelType == type).ToList();
        }
    }

    /// <inheritdoc/>
    public IEnumerable<GameRoom> GetRoomsByGameAndPlatform(TokenGame game, TokenPlatform platform)     {
        lock (this._rooms)
        {
            //Clear all expired rooms
            this.RemoveExpiredRooms();

            //Return a copy of the room list
            return this._rooms.Where(r => r.Game == game && r.Platform == platform).ToList();
        }
    }

    /// <inheritdoc/>
    public GameRoom? GetRoomByUserUuid(ObjectId uuid, TokenPlatform? platform = null, TokenGame? game = null)
    {
        lock (this._rooms)
        {
            this.RemoveExpiredRooms();
            return this._rooms.FirstOrDefault(r =>
                r.PlayerIds.Any(p => p.Id == uuid) &&
                (platform == null || r.Platform == platform) &&
                (game == null || r.Game == game));
        }
    }
    
    /// <inheritdoc/>
    public GameRoom? GetRoomByUsername(string username, TokenPlatform? platform = null, TokenGame? game = null)
    {
        lock (this._rooms)
        {
            this.RemoveExpiredRooms();
            return this._rooms.FirstOrDefault(r =>
                    r.PlayerIds.Any(p => p.Username == username) &&
                    (platform == null || r.Platform == platform) &&
                    (game == null || r.Game == game));
        }
    }
}