using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.Activity.Groups;

[XmlRoot("groups")]
[XmlType("groups")]
public class SerializedActivityGroups
{
    [XmlElement("group")]
    public List<SerializedActivityGroup> Groups { get; set; } = [];
}