using MongoDB.Bson;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

#nullable disable

public partial class LevelCommentRelation : ICommentRelation<GameLevelComment>
{
    [Key] public ObjectId CommentRelationId { get; set; } = ObjectId.GenerateNewId();
    [Required] public GameUser User { get; set; }
    [Required] public GameLevelComment Comment { get; set; }
    public RatingType RatingType { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}