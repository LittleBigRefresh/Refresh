using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Types.Reviews;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Relations;

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
}