using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Activity;

[XmlRoot("events")]
[XmlType("events")]
public class Events
{
    [XmlElement("event")] public List<Event> Items { get; set; } = new();

    public Events(List<Event> items)
    {
        this.Items = items;
    }

    public Events() {}
}