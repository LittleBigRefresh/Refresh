using System.Xml.Serialization;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Database.Models.Users.Leaderboard;

namespace Refresh.GameServer.Types.Lists;

[XmlRoot("scores")]
public class SerializedScoreList
{
    [XmlElement("playRecord")]
    public List<SerializedLeaderboardScore> Scores { get; set; } = new();

    public static SerializedScoreList FromSubmittedEnumerable(IEnumerable<GameSubmittedScore> list, int skip)
    {
        SerializedScoreList value = new();

        int i = skip;
        foreach (GameSubmittedScore score in list)
        {
            i++;
            
            value.Scores.Add(new SerializedLeaderboardScore
            {
                Player = score.Players.FirstOrDefault()?.Username ?? "",
                Score = score.Score,
                Rank = i,
            });
        }

        return value;
    }
}