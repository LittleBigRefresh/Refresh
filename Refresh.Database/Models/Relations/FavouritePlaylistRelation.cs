using Refresh.Database.Models.Playlists;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;
#nullable disable

public partial class FavouritePlaylistRelation : IRealmObject
{
    public GamePlaylist Playlist { get; set; }
    public GameUser User { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}