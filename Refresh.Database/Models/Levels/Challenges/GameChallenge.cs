using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Levels.Challenges;

public partial class GameChallenge : ISequentialId
{
    [Key] public int ChallengeId { get; set; }
    
    public string Name { get; set; } = "";
    public GameUser? Publisher { get; set; }

    #nullable disable
    [Required]
    public GameLevel Level { get; set; }
    #nullable enable

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
    public DateTimeOffset LastUpdateDate { get; set; }
    public DateTimeOffset ExpirationDate { get; set; }

    [NotMapped] public int SequentialId
    {
        get => this.ChallengeId;
        set => this.ChallengeId = value;
    }
}