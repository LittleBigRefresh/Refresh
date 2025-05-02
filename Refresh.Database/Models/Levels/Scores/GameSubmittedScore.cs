using MongoDB.Bson;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.UserData.Leaderboard;

#nullable disable

public partial class GameSubmittedScore : IRealmObject // TODO: Rename to GameScore
{
    [PrimaryKey] public ObjectId ScoreId { get; set; } = ObjectId.GenerateNewId();
    
    // ReSharper disable once InconsistentNaming
    [Indexed] public int _Game { get; set; }
    [Ignored] public TokenGame Game
    {
        get => (TokenGame)this._Game;
        set => this._Game = (int)value;
    }
    
    // ReSharper disable once InconsistentNaming
    public int _Platform { get; set; }
    [Ignored] public TokenPlatform Platform
    {
        get => (TokenPlatform)this._Platform;
        set => this._Platform = (int)value;
    }

    public GameLevel Level { get; set; }
    public IList<GameUser> Players { get; }
    public DateTimeOffset ScoreSubmitted { get; set; }
    
    [Indexed] public int Score { get; set; }
    [Indexed] public byte ScoreType { get; set; }
}