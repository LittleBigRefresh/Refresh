namespace Refresh.Core.RateLimits.Presence;

// level overriding for now
public static class LevelOverrideEndpointLimits
{
    public const int TimeoutDuration = 450;
    public const int RequestAmount = 40;
    public const int BlockDuration = 300;
    public const string RequestBucket = "presence-level-override";
}