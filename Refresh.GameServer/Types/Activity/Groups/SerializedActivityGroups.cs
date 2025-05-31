using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Activity.Groups;

[XmlRoot("groups")]
[XmlType("groups")]
public class SerializedActivityGroups
{
    [XmlElement("group")]
    public IEnumerable<SerializedActivityGroup> Groups { get; set; } = [];
}