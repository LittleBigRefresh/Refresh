using Refresh.Database.Models.Playlists;

namespace Refresh.Database.Models.Relations;

#nullable disable

/// <summary>
/// A mapping of playlist -> sub-playlist
/// </summary>
#if POSTGRES
using PrimaryKeyAttribute = Microsoft.EntityFrameworkCore.PrimaryKeyAttribute;
[PrimaryKey(nameof(PlaylistId), nameof(SubPlaylistId))]
#endif
public partial class SubPlaylistRelation : IRealmObject
{
    /// <summary>
    /// The playlist the level is contained in
    /// </summary>
    [ForeignKey(nameof(PlaylistId))]
    [Required]
    public GamePlaylist Playlist { get; set; }
    /// <summary>
    /// The sub-playlist contained within the playlist
    /// </summary>
    [ForeignKey(nameof(SubPlaylistId))]
    [Required]
    public GamePlaylist SubPlaylist { get; set; }
    
    [Required]
    [Ignored] public int PlaylistId { get; set; }
    [Required]
    [Ignored] public int SubPlaylistId { get; set; }
    
    public DateTimeOffset Timestamp { get; set; }
}