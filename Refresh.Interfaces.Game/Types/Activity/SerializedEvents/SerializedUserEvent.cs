using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.Activity.SerializedEvents;

public class SerializedUserEvent : SerializedEvent
{
    [XmlElement("object_user")]
    public string Username { get; set; } = string.Empty;
}