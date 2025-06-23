using Refresh.Database.Models.Playlists;

namespace Refresh.Database.Models.Relations;

#nullable disable

/// <summary>
/// A mapping of playlist -> sub-playlist
/// </summary>
[PrimaryKey(nameof(PlaylistId), nameof(SubPlaylistId))]
public partial class SubPlaylistRelation
{
    /// <summary>
    /// The playlist the level is contained in
    /// </summary>
    [ForeignKey(nameof(PlaylistId)), Required]
    public GamePlaylist Playlist { get; set; }
    /// <summary>
    /// The sub-playlist contained within the playlist
    /// </summary>
    [ForeignKey(nameof(SubPlaylistId)), Required]
    public GamePlaylist SubPlaylist { get; set; }
    
    [Required] public int PlaylistId { get; set; }
    [Required] public int SubPlaylistId { get; set; }
    
    public DateTimeOffset Timestamp { get; set; }
}