using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Playlists;

namespace Refresh.Database.Models.Relations;

#nullable disable

/// <summary>
/// A mapping of playlist -> sub-level
/// </summary>
#if POSTGRES
using PrimaryKeyAttribute = Microsoft.EntityFrameworkCore.PrimaryKeyAttribute;
[PrimaryKey(nameof(PlaylistId), nameof(LevelId))]
#endif
public partial class LevelPlaylistRelation : IRealmObject
{
    /// <summary>
    /// The playlist the level is contained in
    /// </summary>
    [ForeignKey(nameof(PlaylistId))]
    #if POSTGRES
    [Required]
    #endif
    public GamePlaylist Playlist { get; set; }
    /// <summary>
    /// The level contained within the playlist
    /// </summary>
    [ForeignKey(nameof(LevelId))]
    #if POSTGRES
    [Required]
    #endif
    public GameLevel Level { get; set; }
    
    #if POSTGRES
    [Required]
    #endif
    [Ignored] public int PlaylistId { get; set; }
    #if POSTGRES
    [Required]
    #endif
    [Ignored] public int LevelId { get; set; }
    
    /// <summary>
    /// The place of this level in the playlist, starts from 0
    /// </summary>
    public int Index { get; set; } = 0;
    public DateTimeOffset Timestamp { get; set; }
}