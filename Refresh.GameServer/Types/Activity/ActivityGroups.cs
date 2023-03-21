using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Activity;

[XmlRoot("groups")]
[XmlType("groups")]
public class ActivityGroups
{
    [XmlElement("group")]
    public List<Event> Groups { get; set; } = new();
}