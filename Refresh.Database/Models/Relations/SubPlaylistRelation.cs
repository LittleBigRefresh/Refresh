namespace Refresh.GameServer.Types.Playlists;

/// <summary>
/// A mapping of playlist -> sub-playlist
/// </summary>
public partial class SubPlaylistRelation : IRealmObject
{
    /// <summary>
    /// The playlist the level is contained in
    /// </summary>
    public GamePlaylist Playlist { get; set; }
    /// <summary>
    /// The sub-playlist contained within the playlist
    /// </summary>
    public GamePlaylist SubPlaylist { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}