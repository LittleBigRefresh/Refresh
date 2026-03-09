namespace Refresh.Core.RateLimits.Relations;

public static class LevelTaggingEndpointLimits
{
    public const int TimeoutDuration = 300;
    public const int RequestAmount = 6;
    public const int BlockDuration = 240;
    public const string RequestBucket = "level-tagging";
}