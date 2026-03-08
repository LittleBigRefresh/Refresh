namespace Refresh.Core.RateLimits.Photos;

public static class PhotoListEndpointLimits
{
    public const int TimeoutDuration = 450;
    public const int RequestAmount = 30;
    public const int BlockDuration = 300;
    public const string RequestBucket = "photo-list";
}