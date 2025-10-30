using MongoDB.Bson;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Levels.Challenges;

public partial class GameChallenge : ISequentialId
{
    [Key] public int ChallengeId { get; set; }
    
    public string Name { get; set; } = "";

    [ForeignKey(nameof(PublisherUserId))] public GameUser? Publisher { get; set; }
    public ObjectId? PublisherUserId { get; set; }

    #nullable disable

    [Required, ForeignKey(nameof(LevelId))] public GameLevel Level { get; set; }
    [Required] public int LevelId { get; set; }

    /// <summary>
    /// The Uid of the checkpoint this challenge starts on.
    /// </summary>
    public int StartCheckpointUid { get; set; }
    /// <summary>
    /// The Uid of the checkpoint this challenge finishes on.
    /// </summary>
    public int FinishCheckpointUid { get; set; }
    /// <summary>
    /// The challenge's criteria type (time/score/lives etc).
    /// </summary>
    /// <seealso cref="GameChallengeCriteriaType"/>
    public GameChallengeCriteriaType Type { get; set; }

    public DateTimeOffset PublishDate { get; set; }
    public DateTimeOffset LastUpdateDate { get; set; } // TODO: Consider whether this is even useful
    public DateTimeOffset ExpirationDate { get; set; }

    [NotMapped] public int SequentialId
    {
        get => this.ChallengeId;
        set => this.ChallengeId = value;
    }
}