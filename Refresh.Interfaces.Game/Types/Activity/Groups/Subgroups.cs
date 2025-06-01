using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.Activity.Groups;

[XmlRoot("subgroups")]
[XmlType("subgroups")]
public class Subgroups
{
    [XmlElement("group")] public List<SerializedActivityGroup> Items { get; set; } = [];

    public Subgroups(List<SerializedActivityGroup> items)
    {
        this.Items = items;
    }

    public Subgroups() {}
}