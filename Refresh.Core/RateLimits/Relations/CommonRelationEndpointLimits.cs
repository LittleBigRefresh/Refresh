namespace Refresh.Core.RateLimits.Relations;

public static class CommonRelationEndpointLimits
{
    public const int TimeoutDuration = 300;
    public const int RequestAmount = 70;
    public const int BlockDuration = 240;
    public const string RequestBucket = "common-relations";
}