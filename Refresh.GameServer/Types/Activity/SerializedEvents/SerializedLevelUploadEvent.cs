using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Activity.SerializedEvents;

public class SerializedLevelUploadEvent : SerializedLevelEvent
{
    [XmlElement("republish")]
    public bool Republish { get; set; }
    [XmlElement("count")]
    public int Count { get; set; }

    public static SerializedLevelUploadEvent FromSerializedLevelEvent(SerializedLevelEvent e) => new()
        {
            Count = 0,
            Republish = false,
            
            Actor = e.Actor,
            LevelId = e.LevelId,
            Timestamp = e.Timestamp,
            Type = e.Type,
        };
}