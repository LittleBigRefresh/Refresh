using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Challenges.LbpHub;

#nullable disable

[XmlRoot("slot")]
[XmlType("slot")]
public class SerializedChallengeLevel
{
    [XmlElement("id")] public int LevelId { get; set; }
    [XmlAttribute("type")] public string Type { get; set; }
}