namespace Refresh.Interfaces.Game.Constants.Playlists;

public static class PlaylistEndpointLimits
{
    // rate-limits
    public const int UploadTimeoutDuration = 300;
    public const int MaxCreateAmount = 4; // should be enough
    public const int MaxUpdateAmount = 12;
    public const int UploadBlockDuration = 300;
    public const string CreateBucket = "playlist-create";
    public const string UpdateBucket = "playlist-update";
}