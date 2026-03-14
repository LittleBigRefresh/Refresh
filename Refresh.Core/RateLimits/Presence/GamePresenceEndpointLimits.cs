namespace Refresh.Core.RateLimits.Presence;

/// <summary>
/// Every endpoint related to matching/updating a room in-memory
/// </summary>
public static class GamePresenceEndpointLimits
{
    public const int TimeoutDuration = 450;
    public const int RequestAmount = 30;
    public const int BlockDuration = 300;
    public const string RequestBucket = "game-presence";
}