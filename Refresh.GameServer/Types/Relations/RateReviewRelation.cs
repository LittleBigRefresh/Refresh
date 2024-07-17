using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Types.Reviews;

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
    
    private int _ReviewRatingType { get; set; }
    public GameReview Review { get; set; }
}