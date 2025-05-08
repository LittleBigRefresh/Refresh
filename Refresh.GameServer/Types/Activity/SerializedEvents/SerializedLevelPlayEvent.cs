using System.Xml.Serialization;

namespace Refresh.Database.Models.Activity.SerializedEvents;

public class SerializedLevelPlayEvent : SerializedLevelEvent
{
    /// <summary>
    /// The number of players that played.
    /// </summary>
    [XmlElement("count")]
    public int ScoreType { get; set; }
    
    public static SerializedLevelPlayEvent FromSerializedLevelEvent(SerializedLevelEvent e) => new()
    {
        ScoreType = 1,
            
        Actor = e.Actor,
        LevelId = e.LevelId,
        Timestamp = e.Timestamp,
        Type = e.Type,
    };
}