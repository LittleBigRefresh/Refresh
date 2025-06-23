using MongoDB.Bson;
using Refresh.Database.Models.Comments;
using Refresh.Database.Models.Levels;
using Refresh.Database.Models.Users;

namespace Refresh.Database.Models.Relations;

#nullable disable

public partial class RateLevelRelation
{
    public ObjectId RateLevelRelationId { get; set; } = ObjectId.GenerateNewId();
    
    [NotMapped]
    public RatingType RatingType
    {
        get => (RatingType)this._RatingType;
        set => this._RatingType = (int)value;
    }
    
    // ReSharper disable once InconsistentNaming
    public int _RatingType { get; set; }
    
    [Required] public GameLevel Level { get; set; }
    [Required] public GameUser User { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}