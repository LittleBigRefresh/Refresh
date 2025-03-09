using System.Xml.Serialization;
using Refresh.GameServer.Types.Playlists;

namespace Refresh.GameServer.Types.Lists;

[XmlRoot("playlists")]
[XmlType("playlists")]
public class SerializedLbp3PlaylistList : SerializedList<SerializedLbp3Playlist>
{
    [XmlElement("playlist")]
    public override List<SerializedLbp3Playlist> Items { get; set; } = [];
}