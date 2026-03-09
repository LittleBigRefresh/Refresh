namespace Refresh.Core.RateLimits.Photos;

public static class PhotoListEndpointLimits
{
    public const int TimeoutDuration = 300;
    public const int ApiRequestAmount = 25;
    public const int GameRequestAmount = 60; // LBP1 likes spamming single level + photos for level requests for story levels
    public const int BlockDuration = 240;
    public const string ApiRequestBucket = "photo-list-api";
    public const string GameRequestBucket = "photo-list-game";
}