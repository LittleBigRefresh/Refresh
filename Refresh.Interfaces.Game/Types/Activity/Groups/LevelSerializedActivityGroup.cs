using System.Xml.Serialization;
using Refresh.Interfaces.Game.Types.Levels;

namespace Refresh.Interfaces.Game.Types.Activity.Groups;

public class LevelSerializedActivityGroup : SerializedActivityGroup
{
    [XmlAttribute("type")]
    public override string Type { get; set; } = "level";

    [XmlElement("slot_id")] public SerializedLevelId LevelId { get; set; } = new();
}