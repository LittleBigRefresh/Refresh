using MongoDB.Bson;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

#nullable disable

[PrimaryKey(nameof(Tag), nameof(UserId), nameof(LevelId))]
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
    
    public Tag Tag { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}