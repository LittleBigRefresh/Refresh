using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Activity.SerializedEvents;

[XmlType("event")]
[XmlRoot("event")]
[XmlInclude(typeof(SerializedUserEvent))]
[XmlInclude(typeof(SerializedLevelEvent))]
[XmlInclude(typeof(SerializedLevelUploadEvent))]
[XmlInclude(typeof(SerializedScoreSubmitEvent))]
public abstract class SerializedEvent
{
    [XmlAttribute("type")]
    public EventType Type { get; set; }
    [XmlElement("timestamp")]
    public long Timestamp { get; set; }
    [XmlElement("actor")]
    public string Actor { get; set; } = string.Empty;
}