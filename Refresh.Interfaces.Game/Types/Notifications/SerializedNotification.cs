using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.Notifications;

#nullable disable

[XmlType("notification")]
[XmlRoot("notification")]
public class SerializedNotification
{
    [XmlAttribute("type")] public string Type { get; set; } = "moderationNotification";
    [XmlElement("text")] public string Text { get; set; }
}