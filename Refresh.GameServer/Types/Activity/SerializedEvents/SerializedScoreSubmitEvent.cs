using System.Xml.Serialization;

namespace Refresh.Database.Models.Activity.SerializedEvents;

public class SerializedScoreSubmitEvent : SerializedLevelEvent
{
    [XmlElement("score")]
    public int Score { get; set; }
    
    [XmlElement("count")]
    public int ScoreType { get; set; }
}