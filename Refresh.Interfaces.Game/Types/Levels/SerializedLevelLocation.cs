using System.Xml.Serialization;
using Refresh.Database.Models;
using Refresh.Database.Query;

namespace Refresh.Interfaces.Game.Types.Levels;

#nullable disable

[XmlRoot("slot")]
[XmlType("slot")]
public class SerializedLevelLocation : ISerializedEditLevelLocation
{
    [XmlAttribute("type")] public string Type { get; set; } = "user";
    [XmlElement("id")] public int LevelId { get; set; }
    [XmlElement("location")] public GameLocation Location { get; set; }
}