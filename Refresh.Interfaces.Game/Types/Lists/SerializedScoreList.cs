using System.Xml.Serialization;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Interfaces.Game.Types.UserData.Leaderboard;

namespace Refresh.Interfaces.Game.Types.Lists;

[XmlRoot("scores")]
public class SerializedScoreList
{
    [XmlElement("playRecord")]
    public List<SerializedLeaderboardScore> Scores { get; set; } = new();

    public static SerializedScoreList FromSubmittedEnumerable(IEnumerable<GameScore> list, DataContext dataContext, int skip)
    {
        SerializedScoreList value = new();

        int i = skip;
        foreach (GameScore score in list)
        {
            i++;
            
            value.Scores.Add(new SerializedLeaderboardScore
            {
                Player = dataContext.Database.GetSubmittingPlayerFromScore(score)?.Username ?? "",
                Score = score.Score,
                Rank = i,
            });
        }

        return value;
    }
}