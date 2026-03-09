namespace Refresh.Core.RateLimits.Reviews;

public static class SingleReviewEndpointLimits
{
    public const int TimeoutDuration = 420;
    public const int RequestAmount = 12;
    public const int BlockDuration = 300;
    public const string RequestBucket = "single-review";
}