using Realms;
using Refresh.GameServer.Types.Playlists;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Relations;
#nullable disable

public partial class FavouritePlaylistRelation : IRealmObject
{
    public GamePlaylist Playlist { get; set; }
    public GameUser User { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}