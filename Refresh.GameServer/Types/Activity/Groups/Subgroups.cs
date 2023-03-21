using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Activity.Groups;

[XmlRoot("subgroups")]
[XmlType("subgroups")]
public class Subgroups
{
    [XmlElement("subgroup")] public List<ActivityGroup> Items { get; set; } = new();

    public Subgroups(List<ActivityGroup> items)
    {
        this.Items = items;
    }

    public Subgroups() {}
}