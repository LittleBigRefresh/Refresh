using MongoDB.Bson;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

#nullable disable

[PrimaryKey(nameof(ReviewId), nameof(UserId))]
public partial class RateReviewRelation
{
    [ForeignKey(nameof(ReviewId)), Required]
    public GameReview Review { get; set; }
    [ForeignKey(nameof(UserId)), Required]
    public GameUser User { get; set; }
    
    [Required] public int ReviewId { get; set; }
    [Required] public ObjectId UserId { get; set; }
    
    // we could just reuse RatingType from GameLevel rating logic
    public RatingType RatingType { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}