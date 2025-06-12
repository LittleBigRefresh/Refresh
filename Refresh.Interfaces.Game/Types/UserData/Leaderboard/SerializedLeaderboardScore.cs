using System.Xml.Serialization;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Levels.Scores;

namespace Refresh.Interfaces.Game.Types.UserData.Leaderboard;

#nullable disable

public class SerializedLeaderboardScore
{
    [XmlElement("mainPlayer")] public string Player { get; set; }
    [XmlElement("score")] public int Score { get; set; }
    [XmlElement("rank")] public int Rank { get; set; }

    public static SerializedLeaderboardScore FromOld(GameSubmittedScore score, DataContext dataContext, int rank) => new()
    {
        Player = dataContext.Database.GetSubmittingPlayerFromScore(score)?.Username ?? "",
        Score = score.Score,
        Rank = rank,
    };
}