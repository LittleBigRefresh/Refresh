using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.Levels;

#nullable disable

[XmlRoot("slot"), XmlType("slot")]
public class SerializedLevelResources
{
    [XmlElement("resource")] public string[] Resources { get; set; }
    [XmlAttribute("type")] public string Type { get; set; } = "user";
}