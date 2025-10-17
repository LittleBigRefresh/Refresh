using MongoDB.Bson;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Levels.Challenges;

#nullable disable

public partial class GameChallengeScore
{
    [Key] public ObjectId ScoreId { get; set; } = ObjectId.GenerateNewId();

    [Required, ForeignKey(nameof(ChallengeId))] public GameChallenge Challenge { get; set; }
    [Required] public int ChallengeId { get; set; }
    
    [ForeignKey(nameof(PublisherUserId))] public GameUser Publisher { get; set; }
    [Required] public ObjectId PublisherUserId { get; set; }

    /// <summary>
    /// The publisher's achieved raw score. More always means better here, independent of challenge criteria.
    /// </summary>
    public long Score { get; set; }
    /// <summary>
    /// The hash of the ghost asset for this score.
    /// </summary>
    public string GhostHash { get; set; }
    /// <summary>
    /// The difference between this score's ghost asset's first checkpoint's and last checkpoint's activation time,
    /// in whole seconds. Independent of challenge criteria.
    /// </summary>
    /// <seealso cref="Ghost.SerializedChallengeCheckpoint"/>
    /// <seealso cref="Endpoints.Game.ChallengeEndpoints.SubmitChallengeScore"/>
    public long Time { get; set; }
    public DateTimeOffset PublishDate { get; set; }
}