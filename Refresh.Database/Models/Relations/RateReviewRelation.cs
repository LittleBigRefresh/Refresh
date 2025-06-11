using MongoDB.Bson;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

#nullable disable

#if POSTGRES
using PrimaryKeyAttribute = Microsoft.EntityFrameworkCore.PrimaryKeyAttribute;
[PrimaryKey(nameof(ReviewId), nameof(UserId))]
#endif
public partial class RateReviewRelation : IRealmObject
{
    [ForeignKey(nameof(ReviewId))]
    public GameReview Review { get; set; }
    [ForeignKey(nameof(UserId))]
    public GameUser User { get; set; }
    
    [Ignored] public int ReviewId { get; set; }
    [Ignored] public ObjectId UserId { get; set; }
    
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