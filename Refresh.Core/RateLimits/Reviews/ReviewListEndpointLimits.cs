namespace Refresh.Core.RateLimits.Reviews;

public static class ReviewListEndpointLimits
{
    public const int TimeoutDuration = 420;
    public const int RequestAmount = 20;
    public const int BlockDuration = 300;
    public const string RequestBucket = "review-list";
}