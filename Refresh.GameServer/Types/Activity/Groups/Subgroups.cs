using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Activity.Groups;

[XmlRoot("subgroups")]
[XmlType("subgroups")]
public class Subgroups
{
    [XmlElement("group")] public IEnumerable<SerializedActivityGroup> Items { get; set; } = [];

    public Subgroups(List<SerializedActivityGroup> items)
    {
        this.Items = items;
    }

    public Subgroups() {}
}