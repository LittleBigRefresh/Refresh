using MongoDB.Bson;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Users;

#if POSTGRES
using PrimaryKeyAttribute = Microsoft.EntityFrameworkCore.PrimaryKeyAttribute;
#endif

namespace Refresh.Database.Models.Relations;

#nullable disable

[PrimaryKey(nameof(ReviewId), nameof(UserId))]
public partial class RateReviewRelation : IRealmObject
{
    [ForeignKey(nameof(ReviewId))]
    public GameReview Review { get; set; }
    [ForeignKey(nameof(UserId))]
    public GameUser User { get; set; }
    
    public int ReviewId { get; set; }
    public ObjectId UserId { get; set; }
    
    // we could just reuse RatingType from GameLevel rating logic
    [Ignored, NotMapped]
    public RatingType RatingType
    {
        get => (RatingType)this._ReviewRatingType;
        set => this._ReviewRatingType = (int)value;
    }
    
    // ReSharper disable once InconsistentNaming
    public int _ReviewRatingType { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}