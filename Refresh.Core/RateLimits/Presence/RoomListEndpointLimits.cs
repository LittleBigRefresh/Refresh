namespace Refresh.Core.RateLimits.Presence;

public static class RoomListEndpointLimits
{
    public const int TimeoutDuration = 300;
    public const int RequestAmount = 80; // For now this is being requested many times by refresh-web...
    public const int BlockDuration = 240;
    public const string RequestBucket = "room-list";
}