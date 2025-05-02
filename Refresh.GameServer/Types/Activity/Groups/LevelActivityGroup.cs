using System.Xml.Serialization;
using Refresh.GameServer.Types.Levels;

namespace Refresh.Database.Models.Activity.Groups;

public class LevelActivityGroup : ActivityGroup
{
    [XmlAttribute("type")]
    public override string Type { get; set; } = "level";

    [XmlElement("slot_id")] public SerializedLevelId LevelId { get; set; } = new();
}