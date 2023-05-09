using System.Xml.Serialization;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Types.Lists;

[XmlRoot("scoreboardSegment")]
public class GameScoreSegmentList
{
    [XmlElement("playRecord")]
    public List<GameLeaderboardScore> Scores { get; set; } = new();
    
    public static GameScoreSegmentList FromSubmittedEnumerable(IEnumerable<ScoreWithRank> list)
    {
        GameScoreSegmentList value = new();
        
        foreach (ScoreWithRank score in list)
        {
            value.Scores.Add(new GameLeaderboardScore
            {
                Player = score.score.Players.FirstOrDefault()?.Username ?? "",
                Score = score.score.Score,
                Rank = score.rank,
            });
        }

        return value;
    }
}