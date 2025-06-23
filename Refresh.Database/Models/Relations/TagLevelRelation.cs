using MongoDB.Bson;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

#nullable disable

[PrimaryKey(nameof(_Tag), nameof(UserId), nameof(LevelId))]
public partial class TagLevelRelation
{
    [ForeignKey(nameof(LevelId))]
    [Required]
    public GameLevel Level { get; set; }
    [ForeignKey(nameof(UserId))]
    [Required]
    public GameUser User { get; set; }
    
    [Required] public int LevelId { get; set; }
    [Required] public ObjectId UserId { get; set; }
    
    [NotMapped]
    public Tag Tag
    {
        get => (Tag)this._Tag;
        set => this._Tag = (byte)value;
    }
    
    // ReSharper disable once InconsistentNaming
    public byte _Tag { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}