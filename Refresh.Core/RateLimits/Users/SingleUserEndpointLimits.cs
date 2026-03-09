namespace Refresh.Core.RateLimits.Users;

public static class SingleUserEndpointLimits
{
    public const int TimeoutDuration = 180;
    public const int ApiRequestAmount = 30;
    public const int GameRequestAmount = 50;
    public const int BlockDuration = 120;
    public const string ApiRequestBucket = "single-user-api";
    public const string GameRequestBucket = "single-user-game";
}