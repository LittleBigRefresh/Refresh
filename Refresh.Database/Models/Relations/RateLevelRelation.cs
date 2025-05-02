using MongoDB.Bson;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Reviews;
using Refresh.GameServer.Types.UserData;

namespace Refresh.GameServer.Types.Relations;

#nullable disable

public partial class RateLevelRelation : IRealmObject
{
    public ObjectId RateLevelRelationId { get; set; } = ObjectId.GenerateNewId();
    
    [Ignored]
    public RatingType RatingType
    {
        get => (RatingType)this._RatingType;
        set => this._RatingType = (int)value;
    }
    
    // ReSharper disable once InconsistentNaming
    internal int _RatingType { get; set; }
    
    public GameLevel Level { get; set; }
    public GameUser User { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}