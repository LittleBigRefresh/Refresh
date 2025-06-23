using MongoDB.Bson;
using Refresh.Database.Models.Playlists;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;
#nullable disable

[PrimaryKey(nameof(UserId), nameof(PlaylistId))]
public partial class FavouritePlaylistRelation
{
    [ForeignKey(nameof(PlaylistId))]
    [Required]
    public GamePlaylist Playlist { get; set; }
    [ForeignKey(nameof(UserId))]
    [Required]
    public GameUser User { get; set; }
    
    [Required] public int PlaylistId { get; set; }
    [Required] public ObjectId UserId { get; set; }
    
    public DateTimeOffset Timestamp { get; set; }
}