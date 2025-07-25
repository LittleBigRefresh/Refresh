using System.Xml.Serialization;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Levels.Scores;
using Refresh.Interfaces.Game.Types.UserData.Leaderboard;

namespace Refresh.Interfaces.Game.Types.Lists;

[XmlRoot("scoreboardSegment")]
public class SerializedScoreLeaderboardList
{
    [XmlElement("playRecord")]
    public List<SerializedLeaderboardScore> Scores { get; set; } = new();
    
    public static SerializedScoreLeaderboardList FromDatabaseList(DatabaseList<ScoreWithRank> scoreList, DataContext dataContext)
    {
        SerializedScoreLeaderboardList value = new()
        {
            Scores = scoreList.Items.Select(s => SerializedLeaderboardScore.FromOld(s.score, dataContext, s.rank)).ToList(),
        };

        return value;
    }
}