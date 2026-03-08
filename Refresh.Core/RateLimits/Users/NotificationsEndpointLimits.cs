namespace Refresh.Core.RateLimits.Users;

public static class NotificationsEndpointLimits
{
    public const int TimeoutDuration = 420;
    public const int GameRequestAmount = 5;
    public const int ApiRequestAmount = 20;
    public const int BlockDuration = 300;
    public const string GameRequestBucket = "notifications-game";
    public const string ApiRequestBucket = "notifications-api";
}