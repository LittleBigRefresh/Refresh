namespace Refresh.Core.RateLimits.Playlists;

public static class PlaylistListEndpointLimits
{
    public const int TimeoutDuration = 450;
    public const int RequestAmount = 20;
    public const int BlockDuration = 300;
    public const string RequestBucket = "playlist-list";
}