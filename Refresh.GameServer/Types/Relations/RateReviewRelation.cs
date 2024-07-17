using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Reviews;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Relations;

public partial class RateReviewRelation : IRealmObject
{
    private ObjectId RateReviewRelationId { get; set; } = ObjectId.GenerateNewId();
    
    // we could just reuse RatingType from GameLevel rating logic
    [Ignored]
    public RatingType RatingType
    {
        get => (RatingType)this._RatingType;
        set => this._RatingType = (int)value;
    }
    
    internal int _RatingType { get; set; }
    public GameReview Review { get; set; }
}