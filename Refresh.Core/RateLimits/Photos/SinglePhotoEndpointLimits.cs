namespace Refresh.Core.RateLimits.Photos;

public static class SinglePhotoEndpointLimits
{
    public const int TimeoutDuration = 450;
    public const int RequestAmount = 20;
    public const int BlockDuration = 300;
    public const string RequestBucket = "single-photo";
}