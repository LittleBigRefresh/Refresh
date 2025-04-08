using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Playlists;

[XmlRoot("author")]
[XmlType("author")]
public class SerializedLbp3PlaylistAuthor
{
    public SerializedLbp3PlaylistAuthor() {}
    [XmlElement("npHandle")] public string Username { get; set; } = "";
}