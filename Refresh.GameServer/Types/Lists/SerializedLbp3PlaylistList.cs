using System.Xml.Serialization;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Types.Playlists;

namespace Refresh.GameServer.Types.Lists;

[XmlRoot("playlists")]
[XmlType("playlists")]
public class SerializedLbp3PlaylistList : SerializedList<SerializedLbp3Playlist>
{
    public SerializedLbp3PlaylistList() {}

    public SerializedLbp3PlaylistList(IEnumerable<SerializedLbp3Playlist> list, int total, int skip) 
    {
        this.Items = list.ToList();
        this.Total = total;
        this.NextPageStart = skip + 1;
    }

    [XmlElement("playlist")]
    public override List<SerializedLbp3Playlist> Items { get; set; } = [];
}


// elbeppe 3 moment
[XmlRoot("favouritePlaylists")]
public class SerializedLbp3FavouritePlaylistList : SerializedLbp3PlaylistList
{
    internal SerializedLbp3FavouritePlaylistList() : base() {}
    internal SerializedLbp3FavouritePlaylistList(IEnumerable<SerializedLbp3Playlist> list, int total, int skip) : base(list, total, skip) {}
}