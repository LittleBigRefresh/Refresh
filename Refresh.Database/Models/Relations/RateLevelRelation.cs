using MongoDB.Bson;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

#nullable disable

public partial class RateLevelRelation
{
    public ObjectId RateLevelRelationId { get; set; } = ObjectId.GenerateNewId();
    
    public RatingType RatingType { get; set; }
    
    [Required, ForeignKey(nameof(LevelId))] public GameLevel Level { get; set; }
    [Required] public int LevelId { get; set; }
    [Required, ForeignKey(nameof(UserId))] public GameUser User { get; set; }
    [Required] public ObjectId UserId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}