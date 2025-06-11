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
    public GamePlaylist Playlist { get; set; }
    /// <summary>
    /// The sub-playlist contained within the playlist
    /// </summary>
    [ForeignKey(nameof(SubPlaylistId))]
    public GamePlaylist SubPlaylist { get; set; }
    
    [Ignored] public int PlaylistId { get; set; }
    [Ignored] public int SubPlaylistId { get; set; }
    
    public DateTimeOffset Timestamp { get; set; }
}