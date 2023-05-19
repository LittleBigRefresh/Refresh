using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Types.Levels;

namespace Refresh.GameServer.Types.UserData.Leaderboard;

#nullable disable

public partial class GameSubmittedScore : IRealmObject // TODO: Rename to GameScore
{
    [PrimaryKey] public ObjectId ScoreId { get; set; } = ObjectId.GenerateNewId();
    public GameLevel Level { get; set; }
    public IList<GameUser> Players { get; }
    public DateTimeOffset ScoreSubmitted { get; set; }
    
    [Indexed] public int Score { get; set; }
    [Indexed] public byte ScoreType { get; set; }
}