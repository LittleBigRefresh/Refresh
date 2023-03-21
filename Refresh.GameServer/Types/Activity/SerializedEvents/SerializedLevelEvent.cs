using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Activity.SerializedEvents;

public class SerializedLevelEvent : SerializedEvent
{
    [XmlElement("object_slot_id")]
    public int LevelId { get; set; }
}