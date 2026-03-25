namespace Refresh.Core.RateLimits.Users;

public static class NotificationsEndpointLimits
{
    public const int TimeoutDuration = 180;
    public const int GameRequestAmount = 8;
    public const int ApiRequestAmount = 20;
    public const int BlockDuration = 150;
    public const string GameRequestBucket = "notifications-game";
    public const string ApiRequestBucket = "notifications-api";
}