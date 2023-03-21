using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Activity.Groups;

[XmlRoot("group")]
[XmlType("group")]
[XmlInclude(typeof(LevelActivityGroup))]
[XmlInclude(typeof(UserActivityGroup))]
public abstract class ActivityGroup
{
    [XmlAttribute("type")]
    public abstract string Type { get; set; }
    
    [XmlElement("events")]
    public Events? Events { get; set; }

    [XmlElement("subgroups")]
    public Subgroups? Subgroups { get; set; }
    
    [XmlElement("timestamp")]
    public long Timestamp { get; set; }
}