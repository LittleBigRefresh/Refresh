namespace Refresh.Core.RateLimits.Levels;

public static class LevelListEndpointLimits
{
    public const int TimeoutDuration = 240;
    public const int RequestAmount = 50;
    public const int BlockDuration = 180;
    public const string RequestBucket = "level-list";
}