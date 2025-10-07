using System.Xml.Serialization;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Interfaces.Game.Types.UserData.Leaderboard;

namespace Refresh.Interfaces.Game.Types.Lists;

[XmlRoot("scores")]
public class SerializedScoreList
{
    [XmlElement("playRecord")] public List<SerializedLeaderboardScore> Scores { get; set; } = [];
    [XmlAttribute("totalNumScores")] public int TotalScores { get; set; }
    [XmlAttribute("yourScore")] public int OwnScore { get; set; }
    [XmlAttribute("yourRank")] public int OwnRank { get; set; }

    public static SerializedScoreList FromDatabaseList(DatabaseScoreList scoreList, DataContext dataContext)
    {
        SerializedScoreList value = new()
        {
            Scores = scoreList.Items.Select(s => SerializedLeaderboardScore.FromOld(s.score, s.rank)).ToList(),
            TotalScores = scoreList.TotalItems,
            OwnScore = scoreList.OwnScore?.score.Score ?? -1,
            OwnRank = scoreList.OwnScore?.rank ?? -1,
        };

        return value;
    }
}