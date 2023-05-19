using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Levels;

#nullable disable

[XmlRoot("slot")]
public class SerializedLevelResources
{
    [XmlElement("resource")] public string[] Resources { get; set; }
    [XmlAttribute("type")] public string Type { get; set; } = "user";
}