using MongoDB.Bson;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;

#if POSTGRES
using PrimaryKeyAttribute = Microsoft.EntityFrameworkCore.PrimaryKeyAttribute;
#endif

namespace Refresh.Database.Models.Relations;

#nullable disable

[PrimaryKey(nameof(_Tag), nameof(UserId), nameof(LevelId))]
public partial class TagLevelRelation : IRealmObject
{
    [ForeignKey(nameof(LevelId))]
    public GameLevel Level { get; set; }
    [ForeignKey(nameof(UserId))]
    public GameUser User { get; set; }
    
    public int LevelId { get; set; }
    public ObjectId UserId { get; set; }
    
    [Ignored, NotMapped]
    public Tag Tag
    {
        get => (Tag)this._Tag;
        set => this._Tag = (byte)value;
    }
    
    // ReSharper disable once InconsistentNaming
    public byte _Tag { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}