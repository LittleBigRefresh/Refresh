using System.Xml.Serialization;

namespace Refresh.GameServer.Types.Activity.SerializedEvents;

public class SerializedLevelPlayEvent : SerializedLevelEvent
{
    /// <summary>
    /// The number of players that played.
    /// </summary>
    [XmlElement("count")]
    public int ScoreType { get; set; }
    
    public static SerializedLevelPlayEvent? FromSerializedLevelEvent(SerializedLevelEvent? e)
    {
        if (e == null)
            return null;
        
        return new SerializedLevelPlayEvent
        {
            ScoreType = 1,

            Actor = e.Actor,
            LevelId = e.LevelId,
            Timestamp = e.Timestamp,
            Type = e.Type,
        };
    }
}