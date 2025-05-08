using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

public partial class RateReviewRelation : IRealmObject
{
    // we could just reuse RatingType from GameLevel rating logic
    [Ignored]
    public RatingType RatingType
    {
        get => (RatingType)this._ReviewRatingType;
        set => this._ReviewRatingType = (int)value;
    }
    
    public int _ReviewRatingType { get; set; }
    public GameReview Review { get; set; }
    
    // for the purposes of checking if a positive/negative rating on a review has already been submitted by the user
    public GameUser User { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}