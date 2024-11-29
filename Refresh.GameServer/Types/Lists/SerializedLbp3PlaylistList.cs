using System.Xml.Serialization;
using Refresh.GameServer.Endpoints.Game.DataTypes.Response;
using Refresh.GameServer.Types.Playlists;

namespace Refresh.GameServer.Types.Lists;

#nullable disable

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
    public override List<SerializedLbp3Playlist> Items { get; set; }
}