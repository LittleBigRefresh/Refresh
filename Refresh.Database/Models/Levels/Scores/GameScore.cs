using MongoDB.Bson;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Levels.Scores;

#nullable disable

[Index(nameof(Game), nameof(Score), nameof(ScoreType))]
public partial class GameScore
{
    [Key] public ObjectId ScoreId { get; set; } = ObjectId.GenerateNewId();

    public TokenGame Game { get; set; }
    public TokenPlatform Platform { get; set; }

    [Required] public int LevelId { get; set; }
    [Required] public GameLevel Level { get; set; }
    public DateTimeOffset ScoreSubmitted { get; set; }
    
    public int Score { get; set; }
    public byte ScoreType { get; set; }
    
    public List<string> PlayerIdsRaw { get; set; } = [];
    [NotMapped] public List<ObjectId> PlayerIds => PlayerIdsRaw.Select(ObjectId.Parse).ToList();
    // set => PlayerIdsRaw = value.Select(v => v.ToString()).ToList();

    /// <summary>
    /// The actual publisher of this particular score.
    /// </summary>
    [ForeignKey(nameof(PublisherId)), Required] public GameUser Publisher { get; set; }
    [Required] public ObjectId PublisherId { get; set; }

    /// <summary>
    /// The rank of this score among all scores for the level and score type. Not stored in a separate
    /// entity as that would add unnecessary complexity for now (this is the only stat so far).
    /// Scores which are no longer the high score of their publisher have a rank of 0.
    /// </summary>
    public int Rank { get; set; }
}