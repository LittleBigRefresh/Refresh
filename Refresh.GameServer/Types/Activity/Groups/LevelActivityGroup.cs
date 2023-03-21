using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Activity.Groups;

public class LevelActivityGroup : ActivityGroup
{
    [XmlAttribute("type")]
    public override string Type { get; set; } = "level";

    [XmlElement("slot_id")]
    public int LevelId { get; set; }
}