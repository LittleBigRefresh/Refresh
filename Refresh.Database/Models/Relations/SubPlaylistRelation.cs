using Refresh.Database.Models.Playlists;

#if POSTGRES
using PrimaryKeyAttribute = Microsoft.EntityFrameworkCore.PrimaryKeyAttribute;
#endif

namespace Refresh.Database.Models.Relations;

#nullable disable

/// <summary>
/// A mapping of playlist -> sub-playlist
/// </summary>
[PrimaryKey(nameof(PlaylistId), nameof(SubPlaylistId))]
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
    
    public int PlaylistId { get; set; }
    public int SubPlaylistId { get; set; }
    
    public DateTimeOffset Timestamp { get; set; }
}