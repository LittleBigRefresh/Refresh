using MongoDB.Bson;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

#nullable disable

public partial class PlayLevelRelation
{
    [Key] public ObjectId PlayId { get; set; } = ObjectId.GenerateNewId();
    
    [ForeignKey(nameof(LevelId)), Required]
    public GameLevel Level { get; set; }
    [ForeignKey(nameof(UserId)), Required]
    public GameUser User { get; set; }
    
    [Required] public int LevelId { get; set; }
    [Required] public ObjectId UserId { get; set; }

    public DateTimeOffset Timestamp { get; set; }
    public int Count { get; set; }
}