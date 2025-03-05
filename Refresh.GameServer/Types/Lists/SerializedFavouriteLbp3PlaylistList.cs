using System.Xml.Serialization;
using Refresh.GameServer.Types.Playlists;

namespace Refresh.GameServer.Types.Lists;

[XmlRoot("favouritePlaylists")]
public class SerializedLbp3FavouritePlaylistList : SerializedLbp3PlaylistList
{
    internal SerializedLbp3FavouritePlaylistList() : base() {}
    internal SerializedLbp3FavouritePlaylistList(IEnumerable<SerializedLbp3Playlist> list, int total, int skip) : base(list, total, skip) {}
}