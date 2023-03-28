using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Activity.Groups;

[XmlRoot("groups")]
[XmlType("groups")]
public class ActivityGroups
{
    [XmlElement("group")]
    public List<ActivityGroup> Groups { get; set; } = new();
}