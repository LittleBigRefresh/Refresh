using System.Xml.Serialization;

namespace Refresh.Interfaces.Game.Types.UserData.Leaderboard;

#nullable disable

public class SerializedLeaderboardScore
{
    [XmlElement("mainPlayer")] public string Player { get; set; }
    [XmlElement("score")] public int Score { get; set; }
    [XmlElement("rank")] public int Rank { get; set; }

    public static SerializedLeaderboardScore FromOld(Database.Models.Levels.Scores.GameSubmittedScore score, int rank) => new()
    {
        Player = score.Players[0].Username,
        Score = score.Score,
        Rank = rank,
    };
}