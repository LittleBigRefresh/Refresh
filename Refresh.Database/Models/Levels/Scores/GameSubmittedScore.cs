using MongoDB.Bson;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;

#if POSTGRES
using PrimaryKeyAttribute = Refresh.Database.Compatibility.PrimaryKeyAttribute;
#endif

namespace Refresh.Database.Models.Levels.Scores;

#nullable disable

[Index(nameof(_Game), nameof(Score), nameof(ScoreType))]
public partial class GameSubmittedScore : IRealmObject // TODO: Rename to GameScore
{
    [Key, PrimaryKey] public ObjectId ScoreId { get; set; } = ObjectId.GenerateNewId();
    
    // ReSharper disable once InconsistentNaming
    [Indexed] public int _Game { get; set; }
    [Ignored, NotMapped] public TokenGame Game
    {
        get => (TokenGame)this._Game;
        set => this._Game = (int)value;
    }
    
    // ReSharper disable once InconsistentNaming
    public int _Platform { get; set; }
    [Ignored, NotMapped] public TokenPlatform Platform
    {
        get => (TokenPlatform)this._Platform;
        set => this._Platform = (int)value;
    }

    public GameLevel Level { get; set; }
    public DateTimeOffset ScoreSubmitted { get; set; }
    
    [Indexed] public int Score { get; set; }
    [Indexed] public byte ScoreType { get; set; }
    
    #if !POSTGRES
    public IList<GameUser> Players { get; }
    #else
    public List<string> PlayerIdsRaw { get; set; } = [];
    [NotMapped] public List<ObjectId> PlayerIds => PlayerIdsRaw.Select(ObjectId.Parse).ToList();
    // set => PlayerIdsRaw = value.Select(v => v.ToString()).ToList();
#endif
}