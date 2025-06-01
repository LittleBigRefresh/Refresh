using System.Xml.Serialization;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Levels.Challenges;

namespace Refresh.Interfaces.Game.Types.Challenges.LbpHub;

#nullable disable

[XmlRoot("challenge-score")]
[XmlType("challenge-score")]
public class SerializedChallengeScore : SerializedChallengeAttempt, IDataConvertableFrom<SerializedChallengeScore, Database.Models.Levels.Challenges.GameChallengeScore>
{   
    /// <summary>
    /// This score's rank in the challenge's leaderboard
    /// </summary>
    [XmlElement("rank")] public int Rank { get; set; }
    /// <summary>
    /// The publisher's username.
    /// </summary>
    [XmlElement("player")] public string PublisherName { get; set; }

    #nullable enable

    public static SerializedChallengeScore? FromOld(Database.Models.Levels.Challenges.GameChallengeScore? old, DataContext dataContext)
        => FromOld(old, 0);
    
    public static SerializedChallengeScore? FromOld(GameChallengeScoreWithRank? old)
        => old == null ? null : FromOld(old.score, old.rank);

    public static SerializedChallengeScore? FromOld(Database.Models.Levels.Challenges.GameChallengeScore? old, int rank)
    {
        if (old == null)
            return null;

        return new SerializedChallengeScore
        {
            GhostHash = old.GhostHash,
            Score = old.Score,
            PublisherName = old.Publisher.Username,
            Rank = rank,
        };
    }

    public static IEnumerable<SerializedChallengeScore> FromOldList(IEnumerable<Database.Models.Levels.Challenges.GameChallengeScore> oldList, DataContext dataContext)
        => oldList.Select((s, i) => FromOld(s, i + 1)!);

    public static IEnumerable<SerializedChallengeScore> FromOldList(IEnumerable<GameChallengeScoreWithRank> oldList)
        => oldList.Select(s => FromOld(s)!);
}