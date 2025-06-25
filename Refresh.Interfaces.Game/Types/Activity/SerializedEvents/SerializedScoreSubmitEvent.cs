using System.Xml.Serialization;
using Refresh.Database.Models.Levels.Scores;

namespace Refresh.Interfaces.Game.Types.Activity.SerializedEvents;

public class SerializedScoreSubmitEvent : SerializedLevelEvent
{
    [XmlElement("score")]
    public int Score { get; set; }
    
    [XmlElement("count")]
    public int ScoreType { get; set; }
    
    public static SerializedScoreSubmitEvent? FromSerializedLevelEvent(SerializedLevelEvent? e, GameScore? score)
    {
        if (e == null || score == null)
            return null;
        
        return new SerializedScoreSubmitEvent
        {
            Actor = e.Actor,
            LevelId = e.LevelId,
            Timestamp = e.Timestamp,
            Type = e.Type,

            Score = score.Score,
            ScoreType = score.ScoreType,
        };
    }
}