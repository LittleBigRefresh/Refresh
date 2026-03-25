namespace Refresh.Core.RateLimits.Relations;

public static class PlayLevelEndpointLimits
{
    public const int TimeoutDuration = 300;
    public const int RequestAmount = 40;
    public const int BlockDuration = 240;
    public const string RequestBucket = "play-level";
}