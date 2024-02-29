using Realms;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Reviews;

#nullable disable

public partial class GameReview : IRealmObject, ISequentialId
{
    public int ReviewId { get; set; }
    
    public GameLevel Level { get; set;  }

    public GameUser Publisher { get; set; }
    
    public DateTimeOffset PostedAt { get; set; }
    
    public string Labels { get; set; }
    
    public string Content { get; set; }
    
    public int SequentialId
    {
        set => this.ReviewId = value;
    }
}