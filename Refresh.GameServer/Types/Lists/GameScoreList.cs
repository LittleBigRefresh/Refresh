using System.Xml.Serialization;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Types.Lists;

[XmlRoot("scores")]
public class GameScoreList
{
    [XmlElement("playRecord")]
    public List<GameLeaderboardScore> Scores { get; set; } = new();

    public static GameScoreList FromSubmittedEnumerable(IEnumerable<GameSubmittedScore> list)
    {
        GameScoreList value = new();

        int i = 0;
        foreach (GameSubmittedScore score in list)
        {
            i++;
            
            value.Scores.Add(new GameLeaderboardScore
            {
                Player = score.Players.FirstOrDefault()?.Username ?? "",
                Score = score.Score,
                Rank = i,
            });
        }

        return value;
    }
}