namespace Refresh.Core.RateLimits.Playlists;

/// <summary>
/// smh
/// </summary>
public static class Lbp3PlaylistLevelListEndpointLimits
{
    public const int TimeoutDuration = 450;
    public const int RequestAmount = 200;
    public const int BlockDuration = 300;
    public const string RequestBucket = "playlist-level-list-lbp3";
}