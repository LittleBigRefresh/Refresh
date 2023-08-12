using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Activity.SerializedEvents;

public class SerializedLevelUploadEvent : SerializedLevelEvent
{
    [XmlElement("republish")]
    public bool Republish { get; set; }

    public static SerializedLevelUploadEvent FromSerializedLevelEvent(SerializedLevelEvent e) => new()
        {
            Republish = false,
            
            Actor = e.Actor,
            LevelId = e.LevelId,
            Timestamp = e.Timestamp,
            Type = e.Type,
        };
}