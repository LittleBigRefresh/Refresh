using System.Xml.Serialization;
using Refresh.Common.Constants;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Levels.Challenges;
using Refresh.Database.Query;

namespace Refresh.Interfaces.Game.Types.Challenges.LbpHub;

#nullable disable

[XmlRoot("challenge")]
[XmlType("challenge")]
public class SerializedChallenge : IDataConvertableFrom<SerializedChallenge, GameChallenge>, ICreateChallengeInfo
{
    [XmlElement("id")] public int ChallengeId { get; set; }
    [XmlElement("name")] public string Name { get; set; } = "Unnamed Challenge";
    [XmlElement("slot")] public SerializedChallengeLevel Level { get; set; }
    [XmlElement("author")] public string PublisherName { get; set; } = SystemUsers.DeletedUserName;
    /// <summary>
    /// Always 0, does not seem to affect anything.
    /// </summary>
    [XmlElement("score")] public long Score { get; set; } = 0;
    /// <summary>
    /// The Uid of the checkpoint this challenge starts on.
    /// </summary>
    [XmlElement("start-checkpoint")] public int StartCheckpointUid { get; set; }
    /// <summary>
    /// The Uid of the checkpoint this challenge finishes on.
    /// </summary>
    [XmlElement("end-checkpoint")] public int FinishCheckpointUid { get; set; }
    /// <summary>
    /// Appears to always be 0 when sent by the game.
    /// </summary>
    /// <remarks>
    /// NOTE: This may not be too far behind from now in the response, else Hub will crash.
    /// </remarks>
    [XmlElement("published")] public long PublishedAt { get; set; }
    /// <summary>
    /// Sent by the game as time in days from creation to expiration, which is usually 3, 5 or 7 here, as those are the only selectable options in-game.
    /// For the response we have to send the actual unix milliseconds of the expiration timestamp, else lbp hub will not display the correct
    /// amount of time until expiration
    /// </summary>
    [XmlElement("expires")] public long ExpiresAt { get; set; }

    /// <summary>
    /// An array of criteria of a challenge. Appears to only ever have a single criterion.
    /// </summary>
    /// <seealso cref="SerializedChallengeCriterion"/>
    [XmlArray("criteria")] public List<SerializedChallengeCriterion> Criteria { get; set; } = [];

    public GameChallengeCriteriaType CriteriaType  => this.Criteria
        .Select(c => (GameChallengeCriteriaType)c.Type)
        .First();

    #nullable enable
    public static SerializedChallenge? FromOld(GameChallenge? old, DataContext dataContext)
    {
        if (old == null)
            return null;

        return new() 
        {
            ChallengeId = old.ChallengeId,
            Name = old.Name,
            Level = new SerializedChallengeLevel
            {
                LevelId = old.Level.SlotType == GameSlotType.Story ? old.Level.StoryId : old.Level.LevelId,
                Type = old.Level.SlotType.ToGameType(),
            },
            PublisherName = old.Publisher?.Username ?? SystemUsers.DeletedUserName,
            StartCheckpointUid = old.StartCheckpointUid,
            FinishCheckpointUid = old.FinishCheckpointUid,
            PublishedAt = DateTimeOffset.MaxValue.ToUnixTimeMilliseconds(),
            ExpiresAt = DateTimeOffset.MaxValue.ToUnixTimeMilliseconds(),

            // Take the type of the first (so far always only) criterion in the challenge criteria
            Criteria =
            [
                new()
                {
                    Type = (byte)old.Type
                }
            ],
        };
    }

    public static IEnumerable<SerializedChallenge> FromOldList(IEnumerable<Database.Models.Levels.Challenges.GameChallenge> oldList, DataContext dataContext)
        => oldList.Select(c => FromOld(c, dataContext)!);
}