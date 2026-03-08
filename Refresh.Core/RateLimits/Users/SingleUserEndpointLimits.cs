namespace Refresh.Core.RateLimits.Users;

public static class SingleUserEndpointLimits
{
    public const int TimeoutDuration = 300;
    public const int ApiRequestAmount = 50;
    public const int GameRequestAmount = 90;
    public const int BlockDuration = 240;
    public const string ApiRequestBucket = "single-user-api";
    public const string GameRequestBucket = "single-user-game";
}