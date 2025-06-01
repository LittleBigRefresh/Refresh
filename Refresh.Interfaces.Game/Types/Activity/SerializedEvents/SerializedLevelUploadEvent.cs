using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.Activity.SerializedEvents;

public class SerializedLevelUploadEvent : SerializedLevelEvent
{
    [XmlElement("republish")]
    public bool Republish { get; set; }

    public static SerializedLevelUploadEvent? FromSerializedLevelEvent(SerializedLevelEvent? e)
    {
        if (e == null)
            return null;
        
        return new SerializedLevelUploadEvent
        {
            Republish = false,

            Actor = e.Actor,
            LevelId = e.LevelId,
            Timestamp = e.Timestamp,
            Type = e.Type,
        };
    }
}