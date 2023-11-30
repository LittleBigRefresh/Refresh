using System.Collections.Frozen;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Bunkum.Core.Services;
using MongoDB.Bson;
using NotEnoughLogs;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Services;

/// <summary>
/// Service to assist with storing user friend data in memory.
/// </summary>
/// <remarks>
/// Friend data is stored this way as opposed to in Realm for privacy reasons; we have no right to be harvesting it and storing it.
/// Additionally, this will only store friends who have registered on the server.
/// </remarks>
public class FriendStorageService : EndpointService
{
    public FriendStorageService(Logger logger) : base(logger) {}

    /// <summary>
    /// A dictionary with a list of friends, by the player who friended them.
    /// </summary>
    private readonly Dictionary<ObjectId, FrozenSet<ObjectId>> _friendIdsByPlayer = new();

    public void SetUsersFriends(GameUser user, IEnumerable<GameUser> users)
    {
        this._friendIdsByPlayer.Remove(user.UserId);

        FrozenSet<ObjectId> friendList = users.Select(u => u.UserId)
            .ToFrozenSet();

        this._friendIdsByPlayer.Add(user.UserId, friendList);
    }

    public IEnumerable<GameUser>? GetUsersFriends(GameUser user, IGameDatabaseContext database)
    {
        bool result = this._friendIdsByPlayer.TryGetValue(user.UserId, out FrozenSet<ObjectId>? friendIds);

        if (!result) return null;
        Debug.Assert(friendIds != null);

        return friendIds.Select(i => database.GetUserByObjectId(i))
            .Where(u => u != null)!;
    }
}