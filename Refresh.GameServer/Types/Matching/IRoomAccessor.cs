using MongoDB.Bson;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Endpoints.ApiV3.ApiTypes;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes.Response;
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
    /// Gets a room by a user.
    /// </summary>
    /// <param name="user">The user to search for</param>
    /// <returns>The found room, or null if not found</returns>
    public GameRoom? GetRoomByUser(GameUser user, TokenPlatform? platform = null, TokenGame? game = null) => this.GetRoomByUserUuid(user.UserId);
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