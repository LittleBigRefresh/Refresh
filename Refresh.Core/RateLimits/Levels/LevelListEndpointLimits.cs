namespace Refresh.Core.RateLimits.Levels;

public static class LevelListEndpointLimits
{
    public const int TimeoutDuration = 450;
    public const int RequestAmount = 60;
    public const int BlockDuration = 300;
    public const string RequestBucket = "level-list";
}