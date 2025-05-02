using System.Xml.Serialization;
using Refresh.Database.Models.Activity.SerializedEvents;

namespace Refresh.Database.Models.Activity;

[XmlRoot("events")]
[XmlType("events")]
public class Events
{
    [XmlElement("event")] public List<SerializedEvent> Items { get; set; } = new();

    public Events(List<SerializedEvent> items)
    {
        this.Items = items;
    }

    public Events() {}
}