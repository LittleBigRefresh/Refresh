using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Photos;

#nullable disable

[XmlRoot("slot")]
[XmlType("slot")]
public class SerializedPhotoLevel
{
    [XmlElement("id")] public int LevelId { get; set; }
    [XmlElement("name")] public string Title { get; set; }
    [XmlAttribute("type")] public string Type { get; set; }
}