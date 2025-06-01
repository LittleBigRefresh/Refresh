using System.Xml.Serialization;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Interfaces.Game.Types.UserData.Leaderboard;

namespace Refresh.Interfaces.Game.Types.Lists;

[XmlRoot("scoreboardSegment")]
public class SerializedScoreLeaderboardList
{
    [XmlElement("playRecord")]
    public List<SerializedLeaderboardScore> Scores { get; set; } = new();
    
    public static SerializedScoreLeaderboardList FromSubmittedEnumerable(IEnumerable<ScoreWithRank> list)
    {
        SerializedScoreLeaderboardList value = new();
        
        foreach (ScoreWithRank score in list)
        {
            value.Scores.Add(new SerializedLeaderboardScore
            {
                Player = score.score.Players.FirstOrDefault()?.Username ?? "",
                Score = score.score.Score,
                Rank = score.rank,
            });
        }

        return value;
    }
}