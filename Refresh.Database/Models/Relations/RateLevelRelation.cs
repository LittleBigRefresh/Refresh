using MongoDB.Bson;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

#nullable disable

public partial class RateLevelRelation : IRealmObject
{
    public ObjectId RateLevelRelationId { get; set; } = ObjectId.GenerateNewId();
    
    [Ignored, NotMapped]
    public RatingType RatingType
    {
        get => (RatingType)this._RatingType;
        set => this._RatingType = (int)value;
    }
    
    // ReSharper disable once InconsistentNaming
    public int _RatingType { get; set; }
    
    public GameLevel Level { get; set; }
    public GameUser User { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}