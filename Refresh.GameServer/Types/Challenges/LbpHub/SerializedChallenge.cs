using System.Xml.Serialization;
using Refresh.Common.Constants;
using Refresh.GameServer.Endpoints.ApiV3.DataTypes;
using Refresh.GameServer.Types.Data;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.Challenges.LbpHub;

[XmlRoot("challenge")]
[XmlType("challenge")]
public class SerializedChallenge : IDataConvertableFrom<SerializedChallenge, GameChallenge>
{
    [XmlElement("id")] public int ChallengeId { get; set; }
    [XmlElement("name")] public string Name { get; set; }
    /// <summary>
    /// This challenge's level's type (developer/user) and id (or story id if it is a developer level).
    /// </summary>
    [XmlElement("slot")] public SerializedLevelIdTypeName Level { get; set; }
    [XmlElement("author")] public string PublisherName { get; set; } = SystemUsers.UnknownUserName;
    /// <summary>
    /// Always 0 when challenge is first uploaded by LBP hub, doesn't appear to affect anything if set to not 0 in the response.
    /// The actual first score of a challenge is sent seperately as a SerializedChallengeAttempt.
    /// </summary>
    [XmlElement("score")] public long Score { get; set; }
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
    /// NOTE: For the response we have to send the actual unix milliseconds of the creation timestamp, else lbp hub will crash.
    /// </remarks>
    [XmlElement("published")] public long PublishedAt { get; set; }
    /// <summary>
    /// Sent by the game as time in days from creation to expiration, which is usually 3, 5 or 7 here, as those are the only selectable options in-game.
    /// </summary>
    /// <remarks>
    /// NOTE: For the response we have to send the actual unix milliseconds of the expiration timestamp, else lbp hub will crash.
    /// </remarks>
    [XmlElement("expires")] public long ExpiresAt { get; set; }
    /// <summary>
    /// An array of criteria of a challenge. Appears to only ever have a single criterion.
    /// </summary>
    /// <seealso cref="SerializedChallengeCriterion"/>
    [XmlArray("criteria")] public List<SerializedChallengeCriterion> Criteria { get; set; } = [];

    public static SerializedChallenge? FromOld(GameChallenge? old, DataContext dataContext)
    {
        if (old == null)
            return null;

        return new SerializedChallenge
        {
            ChallengeId = old.ChallengeId,
            Name = old.Name,
            Level = new SerializedLevelIdTypeName
            {
                LevelId = old.Level.SlotType == GameSlotType.Story ? old.Level.StoryId : old.Level.LevelId,
                Type = old.Level.SlotType.ToGameType(),
                Title = old.Level.Title,  // achieves nothing if filled out
            },
            PublisherName = old.Publisher.Username,
            Score = 0,
            StartCheckpointUid = old.StartCheckpointUid,
            FinishCheckpointUid = old.FinishCheckpointUid,
            PublishedAt = old.PublishDate.ToUnixTimeMilliseconds(),
            ExpiresAt = old.ExpirationDate.ToUnixTimeMilliseconds(),
            // Take the type of the first (so far always only) criterion in the challenge criteria
            Criteria =
            [
                new()
                {
                    Type = (byte)old.Type,
                    Value = 0,
                }
            ],
        };
    }

    public static IEnumerable<SerializedChallenge> FromOldList(IEnumerable<GameChallenge> oldList, DataContext dataContext)
        => oldList.Select(c => FromOld(c, dataContext)!);
}