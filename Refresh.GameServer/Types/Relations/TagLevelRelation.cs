using Realms;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Relations;

public partial class TagLevelRelation : IRealmObject
{
    [Ignored]
    public Tag Tag
    {
        get => (Tag)this._Tag;
        set => this._Tag = (byte)value;
    }
    
    public byte _Tag { get; set; }
    
    public GameUser User { get; set; }
    public GameLevel Level { get; set; }
}