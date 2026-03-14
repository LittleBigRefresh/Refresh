namespace Refresh.Core.RateLimits.Presence;

public static class SingleRoomEndpointLimits
{
    public const int TimeoutDuration = 300;
    public const int RequestAmount = 18;
    public const int BlockDuration = 240;
    public const string RequestBucket = "single-room";
}