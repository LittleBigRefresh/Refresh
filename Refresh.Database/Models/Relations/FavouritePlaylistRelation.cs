using MongoDB.Bson;
using Refresh.Database.Models.Playlists;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;
#nullable disable

#if POSTGRES
using PrimaryKeyAttribute = Microsoft.EntityFrameworkCore.PrimaryKeyAttribute;
[PrimaryKey(nameof(UserId), nameof(PlaylistId))]
#endif
public partial class FavouritePlaylistRelation : IRealmObject
{
    [ForeignKey(nameof(PlaylistId))]
    public GamePlaylist Playlist { get; set; }
    [ForeignKey(nameof(UserId))]
    public GameUser User { get; set; }
    
    [Ignored] public int PlaylistId { get; set; }
    [Ignored] public ObjectId UserId { get; set; }
    
    public DateTimeOffset Timestamp { get; set; }
}