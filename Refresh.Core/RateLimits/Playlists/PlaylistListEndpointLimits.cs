namespace Refresh.Core.RateLimits.Playlists;

public static class PlaylistListEndpointLimits
{
    public const int TimeoutDuration = 240;
    public const int RequestAmount = 30;
    public const int Lbp3RequestAmount = 50;
    public const int BlockDuration = 180;
    public const string RequestBucket = "playlist-list";
    public const string Lbp3RequestBucket = "playlist-list-lbp3";
}