using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.Levels;

[XmlRoot("object_slot_id")]
[XmlType("object_slot_id")]
// <object_slot_id type="user">302704</object_slot_id>
public class SerializedLevelId
{
    [XmlText]
    public int LevelId { get; set; }

    [XmlAttribute("type")] public string Type { get; set; } = "user";
}