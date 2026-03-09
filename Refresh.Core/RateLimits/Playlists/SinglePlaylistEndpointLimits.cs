namespace Refresh.Core.RateLimits.Playlists;

public static class SinglePlaylistEndpointLimits
{
    public const int TimeoutDuration = 450;
    public const int RequestAmount = 20;
    public const int BlockDuration = 300;
    public const string RequestBucket = "single-playlist";
}