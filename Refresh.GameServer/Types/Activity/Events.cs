using System.Xml.Serialization;
using Refresh.GameServer.Types.Activity.SerializedEvents;

namespace Refresh.GameServer.Types.Activity;

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