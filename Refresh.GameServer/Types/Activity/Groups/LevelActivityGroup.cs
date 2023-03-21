using System.Xml.Serialization;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.Activity.Groups;

public class LevelActivityGroup : ActivityGroup
{
    [XmlAttribute("type")]
    public override string Type { get; set; } = "level";

    [XmlElement("slot_id")] public GameLevelId LevelId { get; set; } = new();
}