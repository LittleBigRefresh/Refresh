using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Levels;

#nullable disable

[XmlRoot("slot")]
[XmlType("slot")]
public class SerializedLevelLocation
{
    [XmlAttribute("type")] public string Type { get; set; } = "user";
    [XmlElement("id")] public int LevelId { get; set; }
    [XmlElement("location")] public GameLocation Location { get; set; }
}