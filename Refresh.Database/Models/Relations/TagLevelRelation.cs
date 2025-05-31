using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

#nullable disable

public partial class TagLevelRelation : IRealmObject
{
    [Ignored]
    public Tag Tag
    {
        get => (Tag)this._Tag;
        set => this._Tag = (byte)value;
    }
    
    // ReSharper disable once InconsistentNaming
    public byte _Tag { get; set; }
    
    public GameUser User { get; set; }
    public GameLevel Level { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}