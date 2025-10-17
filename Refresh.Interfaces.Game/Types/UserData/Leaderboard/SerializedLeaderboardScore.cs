using System.Xml.Serialization;
using Refresh.Database.Models.Levels.Scores;

namespace Refresh.Interfaces.Game.Types.UserData.Leaderboard;

#nullable disable

public class SerializedLeaderboardScore
{
    [XmlElement("mainPlayer")] public string Player { get; set; }
    [XmlElement("score")] public int Score { get; set; }
    [XmlElement("rank")] public int Rank { get; set; }

    public static SerializedLeaderboardScore FromOld(GameScore score, int rank) => new()
    {
        Player = score.Publisher.Username,
        Score = score.Score,
        Rank = rank,
    };
}