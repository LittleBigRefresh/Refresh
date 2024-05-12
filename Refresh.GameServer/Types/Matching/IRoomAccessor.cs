using MongoDB.Bson;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Matching;

/// <summary>
/// A simple transactional room accessor, rooms returned should be considered read only, and changes must be committed to the IRoomAccessor through UpdateRoom.
/// </summary>
public interface IRoomAccessor
{
    /// <summary>
    /// Gets all currently open rooms.
    /// </summary>
    /// <returns>All open rooms</returns>
    public IEnumerable<GameRoom> GetAllRooms();
    /// <summary>
    /// Gets a room by its UUID.
    /// </summary>
    /// <param name="uuid">The UUID of the room</param>
    /// <returns>The found room</returns>
    public GameRoom? GetRoomByUuid(ObjectId uuid);
    /// <summary>
    /// Adds a room to the list of open rooms, removes any old rooms by same UUID.
    /// </summary>
    /// <param name="room">The room to add</param>
    public void AddRoom(GameRoom room);
    /// <summary>
    /// Removes a room from the list of open rooms by its UUID.
    /// </summary>
    /// <param name="uuid">The UUID to remove</param>
    public void RemoveRoom(ObjectId uuid);
    /// <summary>
    /// Updates any existing room by the same UUID with the new information.
    /// </summary>
    /// <param name="room"></param>
    public void UpdateRoom(GameRoom room);

    /// <summary>
    /// Returns the current match statistics 
    /// </summary>
    /// <returns>The room statistics</returns>
    public RoomStatistics GetStatistics();
    /// <summary>
    /// Get the count of players that are playing a specific game
    /// </summary>
    /// <param name="game">The game to check</param>
    /// <returns>The amount of players</returns>
    public ushort GetPlayersInGame(TokenGame game);
    /// <summary>
    /// Gets the count of players that are playing on a specific platform
    /// </summary>
    /// <param name="platform">The platform to check</param>
    /// <returns>The amount of players</returns>
    public ushort GetPlayersOnPlatform(TokenPlatform platform);
    
    /// <summary>
    /// Gets all the rooms that are on a particular level
    /// </summary>
    /// <param name="type">The type of level</param>
    /// <param name="levelId">The level ID</param>
    /// <returns>The found rooms</returns>
    public IEnumerable<GameRoom> GetRoomsInLevel(RoomSlotType type, int levelId);
    /// <summary>
    /// Gets all the rooms that are on a particular level
    /// </summary>
    /// <param name="level">The level to check</param>
    /// <returns>The found rooms</returns>
    public IEnumerable<GameRoom> GetRoomsInLevel(GameLevel level) => this.GetRoomsInLevel(
        level.Source switch
        {
            GameLevelSource.User => RoomSlotType.Online,
            GameLevelSource.Story => RoomSlotType.Story,
            _ => throw new ArgumentOutOfRangeException(),
        },
        level.LevelId
    );

    /// <summary>
    /// Get all rooms that are open on a particular game and platform
    /// </summary>
    /// <param name="game">The game to check for</param>
    /// <param name="platform">The platform to check for</param>
    /// <returns>The found rooms</returns>
    public IEnumerable<GameRoom> GetRoomsByGameAndPlatform(TokenGame game, TokenPlatform platform);
    /// <summary>
    /// Get all rooms that are open on a particular game and platform, corresponding to the passed token
    /// </summary>
    /// <param name="token">The token to get the game/platform from</param>
    /// <returns>The found rooms</returns>
    public IEnumerable<GameRoom> GetRoomsByGameAndPlatform(Token token) => this.GetRoomsByGameAndPlatform(token.TokenGame, token.TokenPlatform);
    
    /// <summary>
    /// Gets a room by a user.
    /// </summary>
    /// <param name="user">The user to search for</param>
    /// <param name="platform">The platform to check for</param>
    /// <param name="game">The game to check for</param>
    /// <returns>The found room, or null if not found</returns>
    public GameRoom? GetRoomByUser(GameUser user, TokenPlatform? platform = null, TokenGame? game = null) => this.GetRoomByUserUuid(user.UserId, platform, game);
    /// <summary>
    /// Gets a room by a user's UUID.
    /// </summary>
    /// <param name="uuid">The user UUID to search for</param>
    /// <returns>The found room, or null if not found</returns>
    public GameRoom? GetRoomByUserUuid(ObjectId uuid, TokenPlatform? platform = null, TokenGame? game = null);
    /// <summary>
    /// Gets a room by a user's username.
    /// </summary>
    /// <param name="username">The username to search for</param>
    /// <returns>The found room, or null if not found</returns>
    public GameRoom? GetRoomByUsername(string username, TokenPlatform? platform = null, TokenGame? game = null);
}