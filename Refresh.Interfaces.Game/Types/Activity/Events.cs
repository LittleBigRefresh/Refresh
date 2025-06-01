using System.Xml.Serialization;
using Refresh.Interfaces.Game.Types.Activity.SerializedEvents;

namespace Refresh.Interfaces.Game.Types.Activity;

[XmlRoot("events")]
[XmlType("events")]
public class Events
{
    [XmlElement("event")] public List<SerializedEvent> Items { get; set; } = [];

    public Events(List<SerializedEvent> items)
    {
        this.Items = items;
    }

    public Events() {}
}