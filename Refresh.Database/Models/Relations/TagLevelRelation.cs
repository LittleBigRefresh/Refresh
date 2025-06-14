using MongoDB.Bson;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

#nullable disable

#if POSTGRES
using PrimaryKeyAttribute = Microsoft.EntityFrameworkCore.PrimaryKeyAttribute;
[PrimaryKey(nameof(_Tag), nameof(UserId), nameof(LevelId))]
#endif
public partial class TagLevelRelation : IRealmObject
{
    [ForeignKey(nameof(LevelId))]
    #if POSTGRES
    [Required]
    #endif
    public GameLevel Level { get; set; }
    [ForeignKey(nameof(UserId))]
    #if POSTGRES
    [Required]
    #endif
    public GameUser User { get; set; }
    
    #if POSTGRES
    [Required]
    #endif
    [Ignored] public int LevelId { get; set; }
    #if POSTGRES
    [Required]
    #endif
    [Ignored] public ObjectId UserId { get; set; }
    
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