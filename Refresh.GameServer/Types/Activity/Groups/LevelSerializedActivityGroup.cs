using System.Xml.Serialization;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.Activity.Groups;

public class LevelSerializedActivityGroup : SerializedActivityGroup
{
    [XmlAttribute("type")]
    public override string Type { get; set; } = "level";

    [XmlElement("slot_id")] public SerializedLevelId LevelId { get; set; } = new();
}