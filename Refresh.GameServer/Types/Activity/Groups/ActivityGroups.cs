using System.Xml.Serialization;

namespace Refresh.Database.Models.Activity.Groups;

[XmlRoot("groups")]
[XmlType("groups")]
public class ActivityGroups
{
    [XmlElement("group")]
    public List<ActivityGroup> Groups { get; set; } = new();
}