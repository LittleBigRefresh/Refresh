using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Activity.SerializedEvents;

public class SerializedLevelUploadEvent : SerializedLevelEvent
{
    [XmlElement("republish")]
    public bool Republish { get; set; }
    [XmlElement("count")]
    public int Count { get; set; }
}