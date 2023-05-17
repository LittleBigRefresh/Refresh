namespace Refresh.GameServer.Types.Matching;

public enum RoomMood : byte
{
    /// <summary>
    /// The room is rejecting all join requests.
    /// </summary>
    RejectingAll = 0,
    /// <summary>
    /// The room is rejecting all join requests from all users but their friends.
    /// </summary>
    RejectingAllButFriends = 1,
    /// <summary>
    /// The room is rejecting their own friends, but allowing anyone else to join. Why would you even want this?
    /// </summary>
    RejectingOnlyFriends = 2,
    /// <summary>
    /// The room is automatically accepting any join requests.
    /// </summary>
    AllowingAll = 3,
}