namespace Refresh.Core.RateLimits.Playlists;

/// <summary>
/// Shared rate limit parameters for game and API playlist endpoints
/// </summary>
public static class PlaylistCreationEndpointLimits
{
    public const int UploadTimeoutDuration = 300;
    public const int MaxCreateAmount = 30;
    public const int MaxUpdateAmount = 50;
    public const int UploadBlockDuration = 240;
    public const string CreateBucket = "playlist-create";
    public const string UpdateBucket = "playlist-update";
}