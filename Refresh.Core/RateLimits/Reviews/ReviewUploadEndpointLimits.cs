namespace Refresh.Core.RateLimits.Reviews;

public static class ReviewUploadEndpointLimits
{
    public const int TimeoutDuration = 300;
    public const int RequestAmount = 6;
    public const int BlockDuration = 300;
    public const string RequestBucket = "review-upload";
}