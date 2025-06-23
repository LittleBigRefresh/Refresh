using MongoDB.Bson;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Levels.Scores;

#nullable disable

[Index(nameof(_Game), nameof(Score), nameof(ScoreType))]
public partial class GameSubmittedScore // TODO: Rename to GameScore
{
    [Key] public ObjectId ScoreId { get; set; } = ObjectId.GenerateNewId();
    
    // ReSharper disable once InconsistentNaming
    public int _Game { get; set; }
    [NotMapped] public TokenGame Game
    {
        get => (TokenGame)this._Game;
        set => this._Game = (int)value;
    }
    
    // ReSharper disable once InconsistentNaming
    public int _Platform { get; set; }
    [NotMapped] public TokenPlatform Platform
    {
        get => (TokenPlatform)this._Platform;
        set => this._Platform = (int)value;
    }

    [Required] public GameLevel Level { get; set; }
    public DateTimeOffset ScoreSubmitted { get; set; }
    
    public int Score { get; set; }
    public byte ScoreType { get; set; }
    
    public List<string> PlayerIdsRaw { get; set; } = [];
    [NotMapped] public List<ObjectId> PlayerIds => PlayerIdsRaw.Select(ObjectId.Parse).ToList();
    // set => PlayerIdsRaw = value.Select(v => v.ToString()).ToList();
}