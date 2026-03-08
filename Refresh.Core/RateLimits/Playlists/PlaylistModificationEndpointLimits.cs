namespace Refresh.Core.RateLimits.Playlists;

/// <summary>
/// Shared rate limit parameters for game and API playlist endpoints which add/remove levels/playlists
/// </summary>
public static class PlaylistModificationEndpointLimits
{
    public const int TimeoutDuration = 300;
    public const int RequestAmount = 16; // Will increase if this becomes a problem
    public const int BlockDuration = 240;
    public const string RequestBucket = "playlist-modification";
}