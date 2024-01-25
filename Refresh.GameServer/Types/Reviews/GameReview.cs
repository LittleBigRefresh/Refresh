using Realms;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Reviews;

public partial class GameReview : IRealmObject, ISequentialId
{
    [PrimaryKey] 
    public int SequentialId { get; set; }
    
    [Backlink(nameof(GameLevel.Reviews))]
    public IQueryable<GameLevel> Level { get; }

    public GameUser Publisher { get; set; }
    
    public long Timestamp { get; set; }
    
    public string Labels { get; set; }
    
    public string Text { get; set; }
}