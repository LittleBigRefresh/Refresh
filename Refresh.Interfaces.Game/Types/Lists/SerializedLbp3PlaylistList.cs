using System.Xml.Serialization;
using Refresh.Interfaces.Game.Types.Playlists;

namespace Refresh.Interfaces.Game.Types.Lists;

[XmlRoot("playlists")]
[XmlType("playlists")]
public class SerializedLbp3PlaylistList : SerializedList<SerializedLbp3Playlist>
{
    [XmlElement("playlist")]
    public override List<SerializedLbp3Playlist> Items { get; set; } = [];
}