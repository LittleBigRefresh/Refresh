namespace Refresh.Core.RateLimits.Playlists;

/// <summary>
/// smh
/// </summary>
public static class Lbp3PlaylistLevelListEndpointLimits
{
    public const int TimeoutDuration = 360;
    public const int RequestAmount = 80;
    public const int BlockDuration = 260;
    public const string RequestBucket = "playlist-level-list-lbp3";
}