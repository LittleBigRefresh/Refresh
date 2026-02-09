namespace Refresh.Core.RateLimits;

/// <summary>
/// Shared rate limit parameters for game and API playlist endpoints
/// </summary>
public static class PlaylistEndpointLimits
{
    // rate-limits
    public const int UploadTimeoutDuration = 450;
    public const int MaxCreateAmount = 8; // should be enough
    public const int MaxUpdateAmount = 12;
    public const int UploadBlockDuration = 300;
    public const string CreateBucket = "playlist-create";
    public const string UpdateBucket = "playlist-update";
}