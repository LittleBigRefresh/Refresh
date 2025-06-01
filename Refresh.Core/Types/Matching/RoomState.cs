namespace Refresh.Core.Types.Matching;

public enum RoomState
{
    /// <summary>
    /// The room isn't doing much at the moment.
    /// </summary>
    Idle = 0,
    /// <summary>
    /// The room is waiting in a loading screen.
    /// </summary>
    Loading = 1,
    /// <summary>
    /// The room is looking for another group to join.
    /// </summary>
    DivingIn = 3,
    /// <summary>
    /// The room is looking for another group to join them.
    /// </summary>
    WaitingForPlayers = 4,
}