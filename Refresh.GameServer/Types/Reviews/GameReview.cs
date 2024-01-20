using Realms;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Reviews;

public partial class GameReview : IRealmObject, ISequentialId
{
    [PrimaryKey] 
    public int SequentialId { get; set; }
    
    public GameLevel Level { get; set; }

    public GameUser Publisher { get; set; }
    
    public long Timestamp { get; set; }
    
    public string Labels { get; set; }
    
    public bool Deleted { get; set; }

    private int _DeletedBy { get; set; }

    [Ignored]
    public ReviewDeletedBy DeletedBy
    {
        //realm shenanigans
        get => (ReviewDeletedBy)this._DeletedBy; 
        set => this._DeletedBy = (int)value;
    }

    public string Text { get; set; }
}