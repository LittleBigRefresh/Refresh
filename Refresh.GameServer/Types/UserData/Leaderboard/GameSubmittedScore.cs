using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.UserData.Leaderboard;

#nullable disable

public enum GameSubmittedScoreLevelType
{
    Developer,
    User,
}

public partial class GameSubmittedScore : IRealmObject // TODO: Rename to GameScore
{
    [PrimaryKey] public ObjectId ScoreId { get; set; } = ObjectId.GenerateNewId();
    
    // ReSharper disable once InconsistentNaming
    [Indexed] public int _LevelType { get; set; }
    [Ignored] public GameSubmittedScoreLevelType LevelType
    {
        get => (GameSubmittedScoreLevelType)this._LevelType;
        set => this._LevelType = (int)value;
    }
    
    public int? DeveloperId { get; set; }
    [CanBeNull] public GameLevel Level { get; set; }
    public IList<GameUser> Players { get; }
    public DateTimeOffset ScoreSubmitted { get; set; }
    
    [Indexed] public int Score { get; set; }
    [Indexed] public byte ScoreType { get; set; }
}