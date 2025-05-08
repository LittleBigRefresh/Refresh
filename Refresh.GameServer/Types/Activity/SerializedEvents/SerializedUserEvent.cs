using System.Xml.Serialization;

namespace Refresh.Database.Models.Activity.SerializedEvents;

public class SerializedUserEvent : SerializedEvent
{
    [XmlElement("object_user")]
    public string Username { get; set; } = string.Empty;
}