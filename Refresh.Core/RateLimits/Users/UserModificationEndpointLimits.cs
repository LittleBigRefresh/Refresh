namespace Refresh.Core.RateLimits.Users;

public static class UserModificationEndpointLimits
{
    public const int TimeoutDuration = 420;
    public const int ApiRequestAmount = 14;
    public const int GameRequestAmount = 48;
    public const int BlockDuration = 300;
    public const string ApiRequestBucket = "user-modification-api";
    public const string GameRequestBucket = "user-modification-game";
}